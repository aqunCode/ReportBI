using Bi.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bi.Core.Redis
{
    /// <summary>
    /// Redis connection pool.
    /// </summary>
    public class RedisConnectionPoolManager : IRedisConnectionPoolManager
    {
        private static readonly object _lock = new();
        private readonly IConnectionMultiplexer[] _connections;
        private readonly RedisConfiguration _redisConfiguration;
        private readonly ILogger<RedisConnectionPoolManager> _logger;
        private readonly Random _random = new();
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnectionPoolManager"/> class.
        /// </summary>
        /// <param name="redisConfiguration">The redis configuration.</param>
        /// <param name="logger">The logger.</param>
        public RedisConnectionPoolManager(RedisConfiguration redisConfiguration, ILogger<RedisConnectionPoolManager> logger = null)
        {
            this._redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
            this._logger = logger ?? NullLogger<RedisConnectionPoolManager>.Instance;

            if (this._connections.IsNullOrEmpty())
            {
                lock (_lock)
                {
                    if (this._connections.IsNullOrEmpty())
                    {
                        this._connections = new IConnectionMultiplexer[redisConfiguration.PoolSize];
                        this.EmitConnections();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // free managed resources
                foreach (var connection in this._connections)
                    connection?.Dispose();
            }

            _disposed = true;
        }

        /// <inheritdoc/>
        public IConnectionMultiplexer GetConnection()
        {
            var connection = _redisConfiguration.ConnectionSelectionStrategy switch
            {
                ConnectionSelectionStrategy.Random => this._connections[_random.Next(0, _redisConfiguration.PoolSize)],
                ConnectionSelectionStrategy.LeastLoaded => this._connections.OrderBy(x => x.GetCounters().TotalOutstanding).First(),
                _ => throw new Exception(nameof(_redisConfiguration.ConnectionSelectionStrategy))
            };

            _logger.LogDebug("Using redis connection(IConnectionMultiplexer) {0} with {1} outstanding!", connection.GetHashCode(), connection.GetCounters().TotalOutstanding);

            return connection;
        }

        /// <inheritdoc/>
        public ConnectionPoolInformation GetConnectionInformations()
        {
            var activeConnections = 0;
            var invalidConnections = 0;

            var activeConnectionHashCodes = new List<int>();
            var invalidConnectionHashCodes = new List<int>();

            foreach (var connection in _connections)
            {
                if (!connection.IsConnected)
                {
                    invalidConnections++;
                    invalidConnectionHashCodes.Add(connection.GetHashCode());

                    continue;
                }

                activeConnections++;
                activeConnectionHashCodes.Add(connection.GetHashCode());
            }

            return new ConnectionPoolInformation()
            {
                RequiredPoolSize = _redisConfiguration.PoolSize,
                ActiveConnections = activeConnections,
                InvalidConnections = invalidConnections,
                ActiveConnectionHashCodes = activeConnectionHashCodes,
                InvalidConnectionHashCodes = invalidConnectionHashCodes
            };
        }

        private void EmitConnections()
        {
            for (var i = 0; i < this._redisConfiguration.PoolSize; i++)
            {
                IConnectionMultiplexer connection = null;

                if (this._redisConfiguration.ConnectionString.IsNotNullOrEmpty())
                    connection = ConnectionMultiplexer.Connect(
                        this._redisConfiguration.ConnectionString,
                        this._redisConfiguration.Configure,
                        this._redisConfiguration.ConnectLogger);

                if (this._redisConfiguration.ConfigurationOptions != null)
                    connection = ConnectionMultiplexer.Connect(
                        this._redisConfiguration.ConfigurationOptions,
                        this._redisConfiguration.ConnectLogger);

                if (connection == null)
                    throw new Exception($"Create the {i + 1} `IConnectionMultiplexer` connection fail");

                if (this._redisConfiguration.RegisterConnectionEvent)
                {
                    connection.ConnectionFailed +=
                        (s, e) => _logger.LogError(e.Exception, $"Redis(hash:{connection.GetHashCode()}) connection error {e.FailureType}.");

                    connection.ConnectionRestored +=
                        (s, e) => _logger.LogError($"Redis(hash:{connection.GetHashCode()}) connection error restored.");

                    connection.InternalError +=
                        (s, e) => _logger.LogError(e.Exception, $"Redis(hash:{connection.GetHashCode()}) internal error {e.Origin}.");

                    connection.ErrorMessage +=
                        (s, e) => _logger.LogError($"Redis(hash:{connection.GetHashCode()}) error: {e.Message}");
                }

                connection.IncludeDetailInExceptions = true;

                this._redisConfiguration.Action?.Invoke(connection);

                this._connections[i] = connection;
            }
        }
    }
}
