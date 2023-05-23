﻿using Bi.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Bi.Core.Extensions;
/// <summary>
/// IServiceProvider扩展类
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// 根据注入时的唯一名称获取指定的服务，改服务实例必须标注特性 <see cref="ServiceNameAttribute"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this">IServiceProvider</param>
    /// <param name="name">注入时的唯一名称</param>
    /// <returns></returns>
    public static T GetNamedService<T>(this IServiceProvider @this, string name)
    {
        var services = @this.GetServices<T>();
        if (services.IsNullOrEmpty())
            return default;

        return services
                .Where(o =>
                    o.GetType().HasAttribute<ServiceNameAttribute>(x =>
                    x.Name.IsNotNullOrEmpty() &&
                    x.Name.Any(k => k.EqualIgnoreCase(name))))
                .FirstOrDefault();
    }

    /// <summary>
    /// 根据注入时的唯一名称获取指定的服务，改服务实例必须标注特性 <see cref="ServiceNameAttribute"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this">服务实例集合</param>
    /// <param name="name">注入时的唯一名称</param>
    /// <returns></returns>
    public static T GetNamedService<T>(this IEnumerable<T> @this, string name)
    {
        if (@this.IsNullOrEmpty())
            return default;

        return @this
                .Where(o =>
                    o.GetType().HasAttribute<ServiceNameAttribute>(x =>
                    x.Name.IsNotNullOrEmpty() &&
                    x.Name.Any(k => k.EqualIgnoreCase(name))))
                .FirstOrDefault();
    }

    /// <summary>
    /// 根据目标服务类型获取指定的服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this">IServiceProvider</param>
    /// <param name="type">目标服务类型</param>
    /// <returns></returns>
    public static T GetTypedService<T>(this IServiceProvider @this, Type type)
    {
        var services = @this.GetServices<T>();
        if (services.IsNullOrEmpty())
            return default;

        return services.Where(x => x.GetType() == type).FirstOrDefault();
    }

    /// <summary>
    /// 创建实例
    /// </summary>
    /// <param name="this"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static T CreateInstance<T>(this IServiceProvider @this, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<T>(@this, parameters);
    }

    /// <summary>
    /// 创建实例
    /// </summary>
    /// <param name="this"></param>
    /// <param name="type"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object CreateInstance(this IServiceProvider @this, Type type, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance(@this, type, parameters);
    }

    /// <summary>
    /// 获取服务或创建实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <returns></returns>
    public static T GetServiceOrCreateInstance<T>(this IServiceProvider @this)
    {
        return ActivatorUtilities.GetServiceOrCreateInstance<T>(@this);
    }

    /// <summary>
    /// 获取服务或创建实例
    /// </summary>
    /// <param name="this"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetServiceOrCreateInstance(this IServiceProvider @this, Type type)
    {
        return ActivatorUtilities.GetServiceOrCreateInstance(@this, type);
    }
}