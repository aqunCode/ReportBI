using Microsoft.Extensions.DependencyInjection;
using Version = Microsoft.AspNetCore.Mvc.ApiVersion;

namespace Bi.Core.ApiVersion
{
    /// <summary>
    /// ApiVersion扩展类
    /// </summary>
    public static class ApiVersionExtensions
    {
        /// <summary>
        /// ApiVersoin扩展方法
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IServiceCollection AddApiVersion(this IServiceCollection @this)
        {
            @this
                .AddApiVersioning(options =>
                {
                    options.ReportApiVersions = true;
                    options.DefaultApiVersion = Version.Default;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                })
                .AddVersionedApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

            return @this;
        }
    }
}
