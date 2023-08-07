using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Bi.Core.Filters
{
    /// <summary>
    /// 环境变量过滤器，推荐使用：<see cref="TypeFilterAttribute"/>特性标记，若要使用<see cref="ServiceFilterAttribute"/>则Startup中需要注入该过滤器。
    /// </summary>
    /// <remarks>
    ///     <code>
    ///         //无需提前注入
    ///         1.[TypeFilter(typeof(EnvironmentFilter), Arguments = new object[] { new string[] { nameof(Environments.Development) } })]
    ///         
    ///         //需要提前注入：services.AddSingleton(typeof(EnvironmentFilter))
    ///         2.[ServiceFilter(typeof(EnvironmentFilter), Arguments = new object[] { new string[] { nameof(Environments.Development) } })]
    ///     </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class EnvironmentFilter : ActionFilterAttribute
    {
        /// <summary>
        /// 自定义环境变量
        /// </summary>
        public const string APPSETTINGS_ENVIRONMENT = "APPSETTINGS_ENVIRONMENT";

        /// <summary>
        /// 可用的环境变量
        /// </summary>
        private readonly string[] _availableEnvironments;

        /// <summary>
        /// 当前环境变量
        /// </summary>
        private readonly string _nowEnvironment;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="availableEnvironments">可用的环境变量</param>
        public EnvironmentFilter(string[] availableEnvironments)
        {
            _availableEnvironments = availableEnvironments;

            _nowEnvironment = Environment.GetEnvironmentVariable(APPSETTINGS_ENVIRONMENT) ?? Environments.Production;
        }

        /// <summary>
        /// OnActionExecuting
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_availableEnvironments.Any(t => t == _nowEnvironment))
                throw new NotSupportedException($"`{_nowEnvironment}` forbidden access `{context.HttpContext.Request.Path}`");
        }
    }
}
