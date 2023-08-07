using Bi.Core.Caches;
using Bi.Core.Helpers;
using Bi.Core.Redis;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using Pulsar.Client.Api;
using RabbitMQ.Client;
using Scrutor;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bi.Core.Extensions
{
    /// <summary>
    /// IServiceCollection扩展类
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        #region AddFromAssembly
        /// <summary>
        /// 扫描程序集自动注入
        /// </summary>
        /// <param name="this">服务集合</param>
        /// <param name="baseType">基类型，如：typof(IDependency)</param>
        /// <param name="assemblyFilter">程序集过滤器</param>
        /// <param name="typeFilter">程序集中Type过滤器</param>
        /// <param name="lifeTime">生命周期，默认：Transient，其他生命周期可选值：Singleton、Scoped</param>
        /// <returns></returns>
        public static IServiceCollection AddFromAssembly(
            this IServiceCollection @this,
            Type baseType,
            Func<string, bool> assemblyFilter = null,
            Func<Type, bool> typeFilter = null,
            ServiceLifetime lifeTime = ServiceLifetime.Transient)
        {
            //扫描程序集获取指定条件下的类型集合
            var types = AssemblyHelper.GetTypesFromAssembly(filter: assemblyFilter);

            //获取基接口所有继承者
            var inherits = types.Where(x => baseType.IsAssignableFrom(x) && x != baseType).Distinct();
            if (typeFilter.IsNotNull())
                inherits = inherits.Where(typeFilter);

            //获取所有实现类
            var implementationTypes = inherits?.Where(x => x.IsClass);
            if (implementationTypes.IsNotNullOrEmpty())
            {
                foreach (var implementationType in implementationTypes)
                {
                    //获取继承接口
                    var serviceTypes = implementationType.GetInterfaces()?.Where(x => x != baseType);
                    if (serviceTypes.IsNotNullOrEmpty())
                    {
                        foreach (var serviceType in serviceTypes)
                        {
                            switch (lifeTime)
                            {
                                case ServiceLifetime.Singleton:
                                    @this.AddSingleton(serviceType, implementationType);
                                    break;
                                case ServiceLifetime.Transient:
                                    @this.AddTransient(serviceType, implementationType);
                                    break;
                                case ServiceLifetime.Scoped:
                                    @this.AddScoped(serviceType, implementationType);
                                    break;
                                default:
                                    @this.AddTransient(serviceType, implementationType);
                                    break;
                            }
                        }
                    }
                }
            }

            return @this;
        }

        /// <summary>
        /// 扫描程序集自动注入
        /// </summary>
        /// <param name="this"></param>
        /// <param name="typeFilter">程序集中Type过滤器</param>
        /// <param name="assemblyFilter">程序集过滤器</param>
        /// <param name="lifeTime"></param>
        /// <returns></returns>
        public static IServiceCollection AddFromAssembly(
            this IServiceCollection @this,
            Func<IImplementationTypeFilter, IImplementationTypeFilter> typeFilter,
            Func<string, bool> assemblyFilter = null,
            ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        {
            //获取程序集
            var assemblies = AssemblyHelper.GetAssemblies(filter: assemblyFilter);

            //扫描程序集注入接口及实现类
            return @this.Scan(scan => scan
                            .FromAssemblies(assemblies)
                            .AddClasses(classes => typeFilter(classes))
                            .UsingRegistrationStrategy(RegistrationStrategy.Append) //重复注册处理策略，默认Append
                            .AsImplementedInterfaces()
                            .AsSelf()
                            .WithLifetime(lifeTime));
        }
        #endregion

        #region AddWorkerService
        /// <summary>
        /// 自动注入 继承 BackgroundService 的后台服务
        /// </summary>
        /// <param name="this"></param>
        /// <param name="typeFilter">程序集中Type过滤器</param>
        /// <param name="assemblyFilter">程序集过滤器</param>
        /// <param name="lifeTime"></param>
        /// <returns></returns>
        public static IServiceCollection AddWorkerService(
            this IServiceCollection @this,
            IConfiguration configuration)
        {
            var ret = new List<Type>();
            var assemblies = AssemblyHelper.GetAssemblies();
            foreach (var item in assemblies)
            {
                ret.AddRange(item.GetTypes() //获取当前类库下所有类型
                 .Where(t => typeof(BackgroundService).IsAssignableFrom(t)) //获取间接或直接继承t的所有类型
                 .Where(t => !t.IsAbstract && t.IsClass));//获取非抽象类 排除接口继承
            }
            Console.WriteLine($"Find WorkerService:{ret.Select(x => x.Name).Join()}");
            var workerServices = configuration.GetSection("WorkerServices").Get<string[]>() ?? Array.Empty<string>();
            foreach (var item in ret)
            {
                if (workerServices.Contains(item.Name))
                { 
                    @this.AddTransient(typeof(IHostedService), item);
                    Console.WriteLine($"Regist WorkerService:{item.Name}");
                }
            }
            return @this;
        }
        #endregion

        #region AddStackExchangeRedis
        /// <summary>
        /// 注入RedisHelper、ICache、IRedisCacheConnectionPoolManager
        /// </summary>
        /// <param name="this">IServiceCollection</param>
        /// <param name="configuration">appsettings配置</param>
        /// <param name="useElasticApm">是否使用ElasticApm</param>
        /// <param name="log">redis连接日志</param>
        /// <param name="useConnectionPool">是否使用连接池，若为null，则读取配置Redis:UseConnectionPool，若两者均为null，则默认为true</param>
        /// <param name="redisConfiguration">redis连接池配置</param>
        /// <param name="configure">redis连接配置自定义委托</param>
        /// <param name="event">redis会话事件</param>
        /// <returns></returns>
        public static IServiceCollection AddStackExchangeRedis(
            this IServiceCollection @this,
            IConfiguration configuration,
            bool useElasticApm = false,
            TextWriter log = null,
            bool? useConnectionPool = null,
            RedisConfiguration redisConfiguration = null,
            Action<ConfigurationOptions> configure = null,
            EventHandler<RedisProfilingSession> @event = null)
        {
            //判断是否禁用Redis
            if (configuration.GetValue<bool?>("Redis:Enabled") == false)
                return @this;

            var connectionString = configuration.GetValue<string>("Redis:ConnectionStrings");
            if (connectionString.IsNullOrEmpty())
                connectionString = configuration.GetSection("Redis:ConnectionStrings").Get<string[]>()?.FirstOrDefault();

            if (connectionString.IsNullOrEmpty())
                throw new Exception("Redis连接字符串配置为null");

            ConfigHelper.SetConfiguration(configuration);

            //redis数据库索引
            var database = configuration.GetValue<int?>("Redis:Database") ?? 0;

            //是否使用连接池
            useConnectionPool ??= (configuration.GetValue<bool?>("Redis:UseConnectionPool") ?? true);

            //是否启用ElasticApm
            var elasticApmEnabled = configuration.GetValue<bool?>("ElasticApm:Enabled") ?? true;

            //判断是否启用redis连接池
            if (useConnectionPool == false)
            {
                @this.AddTransient(x => new RedisHelper(
                      connectionString,
                      database,
                      null,
                      log));
            }
            else
            {
                //注入redis连接池配置
                @this.AddSingleton(x => redisConfiguration ?? new RedisConfiguration
                {
                    ConnectLogger = log,
                    Configure = configure,
                    ConnectionString = connectionString,
                    PoolSize = configuration.GetValue<int?>("Redis:PoolSize") ?? 5,
                    RegisterConnectionEvent = configuration.GetValue<bool?>("Redis:RegisterEvent") ?? true,
                    Action = null,
                    ConnectionSelectionStrategy = configuration.GetValue<ConnectionSelectionStrategy?>("Redis:ConnectionSelectionStrategy")
                        ?? ConnectionSelectionStrategy.LeastLoaded
                });

                //注入redis连接池
                @this.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();

                //注入RedisHelper
                @this.AddTransient(x => new RedisHelper(database, x.GetRequiredService<IRedisConnectionPoolManager>()));
            }

            //注入内存缓存
            @this.AddMemoryCache();

            //注入ICache
            @this.AddTransient<ICache>(x =>
            {
                return new StackExchangeRedisCache(x.GetRequiredService<RedisHelper>());
            });

            return @this;
        }
        #endregion

        #region AddDistributedLock
        /// <summary>
        /// 注入分布式锁(IDistributedLockProvider)
        /// </summary>
        /// <param name="this"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceCollection AddDistributedLock(
            this IServiceCollection @this,
            IDistributedLockProvider provider)
        {
            return @this.AddSingleton(x => provider);
        }

        /// <summary>
        /// 注入基于Redis的分布式锁(IDistributedLockProvider)
        /// </summary>
        /// <param name="this"></param>
        /// <param name="database"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddDistributedLock(
            this IServiceCollection @this,
            IDatabase database,
            Action<RedisDistributedSynchronizationOptionsBuilder> options = null)
        {
            return @this.AddSingleton<IDistributedLockProvider>(
                x => new RedisDistributedSynchronizationProvider(database, options));
        }

        /// <summary>
        /// 注入基于Redis的分布式锁(IDistributedLockProvider)，注意：此方法需要先注入AddStackExchangeRedis
        /// </summary>
        /// <param name="this"></param>
        /// <param name="defaultDatabase"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddDistributedLock(
            this IServiceCollection @this,
            int defaultDatabase = 0,
            Action<RedisDistributedSynchronizationOptionsBuilder> options = null)
        {
            return @this.AddSingleton<IDistributedLockProvider>(
                x => new RedisDistributedSynchronizationProvider(x
                    .GetRequiredService<RedisHelper>()
                    .RedisConnection
                    .GetDatabase(defaultDatabase),
                    options));
        }
        #endregion

        #region AddMongoDb - 弃用
        ///// <summary>
        ///// 注入MongoDb
        ///// </summary>
        ///// <param name="this"></param>
        ///// <param name="lifeTime">生命周期，默认：单例模式</param>
        ///// <returns></returns>
        //public static IServiceCollection AddMongoDb(
        //    this IServiceCollection @this,
        //    ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        //{
        //    if (ConfigHelper.GetValue<bool?>("Mongodb:Enabled") == false)
        //        return @this;

        //    switch (lifeTime)
        //    {
        //        case ServiceLifetime.Singleton:
        //            @this.AddSingleton(x => new MongodbHelper());
        //            break;
        //        case ServiceLifetime.Scoped:
        //            @this.AddScoped(x => new MongodbHelper());
        //            break;
        //        case ServiceLifetime.Transient:
        //            @this.AddTransient(x => new MongodbHelper());
        //            break;
        //        default:
        //            break;
        //    }
        //    return @this;
        //}

        ///// <summary>
        ///// 注入MongoDb
        ///// </summary>
        ///// <param name="this"></param>
        ///// <param name="databaseName">数据库</param>
        ///// <param name="settings">MongoClientSettings配置</param>
        ///// <param name="lifeTime">生命周期，默认：单例模式</param>
        ///// <returns></returns>
        //public static IServiceCollection AddMongoDb(
        //    this IServiceCollection @this,
        //    string databaseName,
        //    MongoClientSettings settings,
        //    ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        //{
        //    switch (lifeTime)
        //    {
        //        case ServiceLifetime.Singleton:
        //            @this.AddSingleton(x => new MongodbHelper(databaseName, settings));
        //            break;
        //        case ServiceLifetime.Scoped:
        //            @this.AddScoped(x => new MongodbHelper(databaseName, settings));
        //            break;
        //        case ServiceLifetime.Transient:
        //            @this.AddTransient(x => new MongodbHelper(databaseName, settings));
        //            break;
        //        default:
        //            break;
        //    }
        //    return @this;
        //}

        ///// <summary>
        ///// 注入MongoDb
        ///// </summary>
        ///// <param name="this"></param>
        ///// <param name="databaseName">数据库</param>
        ///// <param name="connectionString">连接字符串</param>
        ///// <param name="isMongoClientSettings">是否为MongoClientSettings连接字符串，默认：false</param>
        ///// <param name="lifeTime">生命周期，默认：单例模式</param>
        ///// <returns></returns>
        //public static IServiceCollection AddMongoDb(
        //    this IServiceCollection @this,
        //    string databaseName,
        //    string connectionString,
        //    bool isMongoClientSettings = false,
        //    ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        //{
        //    switch (lifeTime)
        //    {
        //        case ServiceLifetime.Singleton:
        //            @this.AddSingleton(x => new MongodbHelper(databaseName, connectionString, isMongoClientSettings));
        //            break;
        //        case ServiceLifetime.Scoped:
        //            @this.AddScoped(x => new MongodbHelper(databaseName, connectionString, isMongoClientSettings));
        //            break;
        //        case ServiceLifetime.Transient:
        //            @this.AddTransient(x => new MongodbHelper(databaseName, connectionString, isMongoClientSettings));
        //            break;
        //        default:
        //            break;
        //    }
        //    return @this;
        //}
        #endregion

        #region AddRabbitMq
        /// <summary>
        /// 注入RabbitMq
        /// </summary>
        /// <param name="this"></param>
        /// <param name="factory">连接工厂配置</param>
        /// <param name="lifeTime">生命周期，默认：单例模式</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection @this,
            ConnectionFactory factory,
            ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        {
            switch (lifeTime)
            {
                case ServiceLifetime.Singleton:
                    @this.AddSingleton(x => new RabbitMqHelper(factory));
                    break;
                case ServiceLifetime.Scoped:
                    @this.AddScoped(x => new RabbitMqHelper(factory));
                    break;
                case ServiceLifetime.Transient:
                    @this.AddTransient(x => new RabbitMqHelper(factory));
                    break;
                default:
                    break;
            }
            return @this;
        }

        /// <summary>
        /// 注入RabbitMq
        /// </summary>
        /// <param name="this"></param>
        /// <param name="config">连接配置</param>
        /// <param name="lifeTime">生命周期，默认：单例模式</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection @this,
            MqConfig config,
            ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        {
            switch (lifeTime)
            {
                case ServiceLifetime.Singleton:
                    @this.AddSingleton(x => new RabbitMqHelper(config));
                    break;
                case ServiceLifetime.Scoped:
                    @this.AddScoped(x => new RabbitMqHelper(config));
                    break;
                case ServiceLifetime.Transient:
                    @this.AddTransient(x => new RabbitMqHelper(config));
                    break;
                default:
                    break;
            }
            return @this;
        }

        /// <summary>
        /// 注入RabbitMq
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration">json配置</param>
        /// <param name="lifeTime">生命周期，默认：单例模式</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection @this,
            IConfiguration configuration,
            ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        {
            switch (lifeTime)
            {
                case ServiceLifetime.Singleton:
                    @this.AddSingleton(x => new RabbitMqHelper(configuration.GetSection("RabbitMq").Get<MqConfig>()));
                    break;
                case ServiceLifetime.Scoped:
                    @this.AddScoped(x => new RabbitMqHelper(configuration.GetSection("RabbitMq").Get<MqConfig>()));
                    break;
                case ServiceLifetime.Transient:
                    @this.AddTransient(x => new RabbitMqHelper(configuration.GetSection("RabbitMq").Get<MqConfig>()));
                    break;
                default:
                    break;
            }
            return @this;
        }
        #endregion

        #region AddIf
        /// <summary>
        /// 根据条件注入服务
        /// </summary>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddIf(
            this IServiceCollection @this,
            bool condition,
            Action<IServiceCollection> action)
        {
            if (condition && action != null)
                action(@this);

            return @this;
        }
        #endregion
    }
}
