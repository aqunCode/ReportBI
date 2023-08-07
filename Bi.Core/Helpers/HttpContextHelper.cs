using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// HttpContext工具类
    /// </summary>
    public static class HttpContextHelper
    {
        /// <summary>
        /// IHttpContextAccessor
        /// </summary>
        private static IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 使用HttpContext，Startup中配置IHttpContextAccessor
        /// </summary>
        /// <param name="app"></param>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services)
        ///         {
        ///             services.TryAddSingleton$lt;IHttpContextAccessor, HttpContextAccessor&gt;();
        ///             services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        ///         }
        ///         
        ///         public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider svp)
        ///         {
        ///             loggerFactory.AddConsole(Configuration.GetSection("Logging"));
        ///             loggerFactory.AddDebug();
        ///             //配置HttpContext
        ///             app.UseHttpContext();
        ///         }
        ///     </code>
        /// </example>
        public static void UseHttpContext(this IApplicationBuilder app)
        {
            _httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        }

        /// <summary>
        /// 当前HttpContext
        /// </summary>
        public static HttpContext Current => _httpContextAccessor.HttpContext;
    }
}
