using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Bi.Core.Middleware
{
    /// <summary>
    /// 自定义异常处理中间件
    /// </summary>
    public class ExceptionHandlerMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExceptionHandlerMiddleWare> _logger;

        public ExceptionHandlerMiddleWare(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<ExceptionHandlerMiddleWare> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await context.HandleExceptionAsync(ex, _configuration, _logger);
            }
        }
    }
}
