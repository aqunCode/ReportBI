using Bi.Core.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Bi.Core.Models
{
    /// <summary>
    /// 全局静态类
    /// </summary>
    public static class App
    {
        /// <summary>
        /// 服务集合
        /// </summary>
        public static IServiceCollection Services { get; set; }
        /// <summary>
        /// 根服务，StartUp中进行初始化赋值(单例直接使用这个获取)
        /// </summary>
        public static IServiceProvider RootServices { get; set; }
        public static IWebHostEnvironment WebHostEnvironment => RootServices?.GetService<IWebHostEnvironment>();
        public static HttpContext HttpContext => RootServices?.GetService<IHttpContextAccessor>()?.HttpContext;
        /// <summary>
        /// 获取请求生存周期的服务(未注册返回null)
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static TService GetService<TService>(IServiceProvider serviceProvider = null) where TService : class
        {
            return GetService(typeof(TService), serviceProvider) as TService;
        }
        /// <summary>
        /// 获取请求生存周期的服务(未注册返回null)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static object GetService(Type type, IServiceProvider serviceProvider = null)
        {
            return (serviceProvider ?? GetServiceProvider(type)).GetService(type);
        }
        /// <summary>
        /// 获取请求生存周期的服务(未注册异常)
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static TService GetRequiredService<TService>(IServiceProvider serviceProvider = null) where TService : class
        {
            return GetRequiredService(typeof(TService), serviceProvider) as TService;
        }
        /// <summary>
        /// 获取请求生存周期的服务(未注册异常)
        /// </summary>
        /// <typeparam name="type"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static object GetRequiredService(Type type, IServiceProvider serviceProvider = null)
        {
            return (serviceProvider ?? GetServiceProvider(type)).GetRequiredService(type);
        }
        /// <summary>
        /// 获取服务注册器
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(Type serviceType)
        {
            if (WebHostEnvironment == null)
            {
                return RootServices;
            }
            if (RootServices != null && Services.Where((ServiceDescriptor u) => u.ServiceType == (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType)).Any((ServiceDescriptor u) => u.Lifetime == ServiceLifetime.Singleton))
            {
                return RootServices;
            }
            if (HttpContext?.RequestServices != null)
            {
                return HttpContext.RequestServices;
            }
            return RootServices;
        }
    }
}
