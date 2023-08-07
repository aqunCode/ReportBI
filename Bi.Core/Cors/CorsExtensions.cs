using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bi.Core.Cors
{
    /// <summary>
    /// 跨域扩展类
    /// </summary>
    public static class CorsExtensions
    {
        /// <summary>
        /// 自定义策略跨域
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IServiceCollection AddPolicyCors(this IServiceCollection @this)
        {
            @this.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", x =>
                {
                    x.AllowAnyHeader();
                    x.AllowAnyMethod();
                    x.SetIsOriginAllowed(_ => true);
                    x.AllowCredentials();
                });
            });

            return @this;
        }

        /// <summary>
        /// 使用自定义策略跨域
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IApplicationBuilder UsePolicyCors(this IApplicationBuilder @this)
        {
            @this.UseCors("CorsPolicy");

            return @this;
        }
    }
}
