using Bi.Core.Exceptions;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Middleware
{
	/// <summary>
	/// 全局异常处理扩展类
	/// </summary>
	public static class ExceptionHandlerExtensions
    {

        /// <summary>
        /// 全局异常处理
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionHandler(
            this IApplicationBuilder @this,
            IConfiguration configuration,
            ILogger logger)
        {
            return @this.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (ex.IsNotNull())
                        await context.HandleExceptionAsync(ex, configuration, logger);
                }
            });
        }

        /// <summary>
        /// 全局异常中间件
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder @this)
        {
            return @this.UseMiddleware<ExceptionHandlerMiddleWare>();
        }

        /// <summary>
        /// 处理异常
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task HandleExceptionAsync(
            this HttpContext context,
            Exception ex,
            IConfiguration configuration,
            ILogger logger)
        {
            //初始化返回结果
            var retval = new ResponseResult<string>();

            //判断异常是否为自定义
            var isTips = typeof(TipsException) == ex.GetType();

            //判断返回状态码
            if (isTips)
                retval.Code = ResponseCode.Error;
            else if (ex.GetType() == typeof(InvalidOperationException))
                retval.Code = ResponseCode.Unauthorized;
            else
            {
                retval.Code = ResponseCode.InternalServerError;
                context.Response.Headers.Add("exception", "bi-exception-handler");
            }

            //自定义异常
            if (isTips)
            {
                //判断错误信息是否为错误码
                if (ex.Message.IsFloat())
                    retval.ErrorCode = ex.Message.ToDouble();
                else
                    retval.Message = ex.Message;
            }
            else
            {
#if DEBUG
                //开发模式提示具体异常
                retval.Message = ex.Message;
#else
                //未来正式稳定后此代码需要注释掉
                retval.Message = ex.Message;
#endif
                if (!isTips)
                {
                    logger.LogError(ex, "系统错误，由全局异常中间件拦截");
                }
            }

            //返回结果
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(retval.ToJson(true), Encoding.UTF8);
        }
    }
}
