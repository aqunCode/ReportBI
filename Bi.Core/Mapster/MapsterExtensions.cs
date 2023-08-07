using Bi.Core.Extensions;
using FastExpressionCompiler;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Bi.Core.Mapster
{
    /// <summary>
    /// Mapster扩展类
    /// </summary>
    public static class MapsterExtensions
    {
        /// <summary>
        /// 注入并初始化Mapster
        /// </summary>
        /// <param name="this"></param>
        /// <param name="enableCompileFast">是否启用FastExpressionCompiler，默认不启用</param>
        /// <param name="mapsterConfig">自定义全局配置</param>
        /// <param name="assemblies">扫描程序集，扫描所有继承IRegister的配置类</param>
        /// <returns></returns>
        public static IServiceCollection AddMapster(
            this IServiceCollection @this,
            bool enableCompileFast = false,
            Action<TypeAdapterConfig> mapsterConfig = null,
            params Assembly[] assemblies)
        {
            var config = TypeAdapterConfig.GlobalSettings;

            if (enableCompileFast)
                config.Compiler = exp => exp.CompileFast();

            config.EnableJsonMapping();
            config.Default.EnumMappingStrategy(EnumMappingStrategy.ByName);
            config.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

            if (assemblies.IsNotNullOrEmpty())
                config.Scan(assemblies);

            mapsterConfig?.Invoke(config);

            @this.AddSingleton(config);
            @this.AddSingleton<IMapper, ServiceMapper>();

            return @this;
        }
    }
}