﻿using Bi.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// Config工具类
    /// </summary>
    public class ConfigHelper
    {
        #region Public Property
        /// <summary>
        /// app配置
        /// </summary>
        public static IConfiguration Configuration { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ConfigHelper()
        {
            var jsonFile = "appsettings.json";
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment.IsNotNullOrEmpty())
                jsonFile = $"appsettings.{environment}.json";

            //添加appsettings自定义环境变量
            var env = Environment.GetEnvironmentVariable("APPSETTINGS_ENVIRONMENT");
            if (env.IsNotNullOrWhiteSpace())
                jsonFile = $"appsettings.{env}.json";

            SetConfiguration(jsonFile);
        }
        #endregion

        #region SetConfiguration
        /// <summary>
        /// 设置app配置
        /// </summary>
        /// <param name="configuration">配置</param>
        public static void SetConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 设置app配置(json文件)
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="basePath">文件路径，默认：Directory.GetCurrentDirectory()</param>
        public static void SetConfiguration(string fileName, string basePath = null)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(string.IsNullOrEmpty(basePath) ? Directory.GetCurrentDirectory() : basePath)
                .AddJsonFile(fileName, optional: true, reloadOnChange: true)
                .Build();
        }

        /// <summary>
        /// 设置app配置(Apollo)
        /// </summary>
        /// <param name="appId">Apollo的AppId</param>
        /// <param name="metaServer">Apollo的配置中心地址</param>
        /// <param name="nameSpace">Apollo命名空间</param>
        public static void SetConfiguration(string appId, string metaServer, string nameSpace)
        {
            Configuration = new ConfigurationBuilder()
                //.AddApollo(appId, metaServer)
                //.AddNamespace(nameSpace, ConfigFileFormat.Json)
                .Build();
        }

        ///// <summary>
        ///// 设置app配置(Apollo)
        ///// </summary>
        ///// <param name="appId">Apollo的AppId</param>
        ///// <param name="metaServer">Apollo的配置中心地址</param>
        ///// <param name="action">Apollo自定义委托</param>
        //public static void SetConfiguration(string appId, string metaServer, Action<IApolloConfigurationBuilder> action)
        //{
        //    var builder = new ConfigurationBuilder()
        //        .AddApollo(appId, metaServer);

        //    action?.Invoke(builder);

        //    Configuration = builder.Build();
        //}

        ///// <summary>
        ///// 设置app配置(AgileConfig)
        ///// </summary>
        ///// <param name="client">AgileConfig客户端配置</param>
        ///// <param name="action">AgileConfig自定义委托</param>
        //public static void SetConfiguration(ConfigClient client, Action<ConfigChangedArg> action)
        //{
        //    Configuration = new ConfigurationBuilder()
        //        .AddAgileConfig(client, action)
        //        .Build();
        //}

        ///// <summary>
        ///// 设置app配置(AgileConfig)
        ///// </summary>
        ///// <param name="appId">AgileConfig客户端appId</param>
        ///// <param name="secret">AgileConfig客户端secret</param>
        ///// <param name="serverNodes">AgileConfig客户端serverNodes</param>
        ///// <param name="env">AgileConfig开发环境</param>
        ///// <param name="action">AgileConfig自定义委托</param>
        ///// <param name="logger">ILogger日志</param>
        //public static void SetConfiguration(string appId, string secret, string serverNodes, string env, Action<ConfigChangedArg> action, ILogger logger = null)
        //{
        //    var client = new ConfigClient(appId, secret, serverNodes, env, logger);

        //    Configuration = new ConfigurationBuilder()
        //        .AddAgileConfig(client, action)
        //        .Build();
        //}
        #endregion

        #region Get
        /// <summary>
        /// 根据key值获取Section然后转换为T类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return Configuration.GetSection(key).Get<T>();
        }

        /// <summary>
        /// 根据key值获取Section然后转换为T类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static T Get<T>(string key, Action<BinderOptions> configureOptions)
        {
            return Configuration.GetSection(key).Get<T>(configureOptions);
        }
        #endregion

        #region Bind
        /// <summary>
        /// 绑定配置到已有实例
        /// </summary>
        /// <param name="key"></param>
        /// <param name="instance"></param>
        public static void Bind(string key, object instance)
        {
            Configuration.GetSection(key).Bind(instance);
        }

        /// <summary>
        /// 绑定配置到已有实例
        /// </summary>
        /// <param name="key"></param>
        /// <param name="instance"></param>
        /// <param name="configureOptions"></param>
        public static void Bind(string key, object instance, Action<BinderOptions> configureOptions)
        {
            Configuration.GetSection(key).Bind(instance, configureOptions);
        }
        #endregion

        #region GetValue
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetValue<T>(string key)
        {
            return Configuration.GetValue<T>(key);
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetValue<T>(string key, T defaultValue)
        {
            return Configuration.GetValue<T>(key, defaultValue);
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="type">配置对象类型</param>
        /// <param name="key">配置名称</param>
        /// <returns>返回object类型配置对象</returns>
        public static object GetValue(Type type, string key)
        {
            return Configuration.GetValue(type, key);
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="type">配置对象类型</param>
        /// <param name="key">配置名称</param>
        /// <param name="defaultValue"></param>
        /// <returns>返回object类型配置对象</returns>
        public static object GetValue(Type type, string key, object defaultValue)
        {
            return Configuration.GetValue(type, key, defaultValue);
        }
        #endregion

        #region GetConnectionString
        /// <summary>
        /// 获取ConnectionStrings节点下的链接字符串
        /// </summary>
        /// <param name="name">连接字符串名称</param>
        /// <returns>返回连接字符串</returns>
        public static string GetConnectionString(string name)
        {
            return Configuration.GetConnectionString(name);
        }
        #endregion

        #region GetOptions
        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptions<T>() where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration)
                .BuildServiceProvider()
                .GetService<IOptions<T>>();

            return options?.Value;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptions<T>(string key) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptions<T>>();

            return options?.Value;
        }
        #endregion

        #region GetOptionsMonitor
        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitor<T>() where T : class, new()
        {
            var options = new ServiceCollection()
               .Configure<T>(Configuration)
               .BuildServiceProvider()
               .GetService<IOptionsMonitor<T>>();

            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitor<T>(string key) where T : class, new()
        {
            var options = new ServiceCollection()
               .Configure<T>(Configuration.GetSection(key))
               .BuildServiceProvider()
               .GetService<IOptionsMonitor<T>>();

            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="listener">监听配置变化时的委托</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitor<T>(Action<T> listener) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration)
                .BuildServiceProvider()
                .GetService<IOptionsMonitor<T>>();

            if (listener != null)
                options?.OnChange(listener);

            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="listener">监听配置变化时的委托</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitor<T>(Action<T, string> listener) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration)
                .BuildServiceProvider()
                .GetService<IOptionsMonitor<T>>();

            if (listener != null)
                options?.OnChange(listener);

            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <param name="listener">监听配置变化时的委托</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitor<T>(string key, Action<T> listener) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptionsMonitor<T>>();

            if (listener != null)
                options?.OnChange(listener);

            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <param name="listener">监听配置变化时的委托</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitor<T>(string key, Action<T, string> listener) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptionsMonitor<T>>();

            if (listener != null)
                options?.OnChange(listener);

            return options?.CurrentValue;
        }
        #endregion
    }
}
