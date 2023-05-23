using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Redis;
/// <summary>
/// The redis configuration
/// </summary>
public class RedisConfiguration
{
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the Redis configuration options
    /// </summary>
    /// <value>An instanfe of <see cref="ConfigurationOptions" />.</value>
    public ConfigurationOptions ConfigurationOptions { get; set; }

    /// <summary>
    /// Gets or sets redis connections pool size.
    /// </summary>
    public int PoolSize { get; set; }

    /// <summary>
    /// Gets or sets the action to IConnectionMultiplexer.
    /// </summary>
    public Action<IConnectionMultiplexer> Action { get; set; }

    /// <summary>
    /// Gets or sets the Redis configuration options,only used when `ConnectionString` is not null.
    /// </summary>
    /// <remarks>
    ///     <code>
    ///         var conn = ConnectionMultiplexer.Connect(
    ///             redisConnectionString.ConnectionString, 
    ///             options => options.SocketManager = SocketManager.ThreadPool);
    ///     </code>
    /// </remarks>
    public Action<ConfigurationOptions> Configure { get; set; }

    /// <summary>
    /// Gets or sets redis `ConnectionMultiplexer.Connect` log parameter
    /// </summary>
    public TextWriter ConnectLogger { get; set; }

    /// <summary>
    /// Gets or sets IConnectionMultiplexer event
    /// </summary>
    public bool RegisterConnectionEvent { get; set; } = true;

    /// <summary>
    /// Gets or sets the every ConnectionSelectionStrategy to use during connection selection,the default is `LeastLoaded`.
    /// </summary>
    public ConnectionSelectionStrategy ConnectionSelectionStrategy { get; set; } =
        ConnectionSelectionStrategy.LeastLoaded;
}