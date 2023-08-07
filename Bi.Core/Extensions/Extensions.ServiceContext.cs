using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bi.Core.Extensions
{
    /// <summary>
    /// ServiceContext扩展类
    /// </summary>
    public static class ServiceContextExtensions
    {
        #region Trace
        #region Sync
        /// <summary>
        /// 获取TraceId，metadata键必须为traceid
        /// </summary>
        /// <param name="this"></param>
        /// <param name="traceKey">跟踪请求头部key</param>
        /// <returns></returns>
        public static string GetTraceId(this ServiceContext @this, string traceKey = "traceid") =>
            @this?.CallContext.RequestHeaders.Get(traceKey)?.Value;

        /// <summary>
        /// 开始跟踪
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="logger">日志</param>
        /// <param name="function">执行方法名称</param>
        /// <param name="traceKey">跟踪请求头部key</param>
        public static void BeginTrace(this ServiceContext @this, ILogger logger, string function, string traceKey = "traceid") =>
            @this.Trace(logger, x => $"traceId -> {x} -> `Begin {function}`.", traceKey);

        /// <summary>
        /// 结束跟踪
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="logger">日志</param>
        /// <param name="function">执行方法名称</param>
        /// <param name="traceKey">跟踪请求头部key</param>
        /// <param name="writeTraceId">是否写入traceId到响应headers中</param>
        public static void EndTrace(this ServiceContext @this, ILogger logger, string function, string traceKey = "traceid", bool writeTraceId = true)
        {
            @this.Trace(logger, x =>
            {
                if (writeTraceId)
                    @this.WriteHeaders(x);

                return $"traceId -> {x} -> `End {function}`.";

            }, traceKey);
        }

        /// <summary>
        /// 日志跟踪
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="logger">日志</param>
        /// <param name="message">自定义跟踪消息委托，参数TraceId</param>
        /// <param name="traceKey">跟踪请求头部key</param>
        public static void Trace(this ServiceContext @this, ILogger logger, Func<string, string> message, string traceKey = "traceid")
        {
            var traceId = @this.GetTraceId(traceKey);

            if (message.IsNotNull() && traceId.IsNotNullOrEmpty())
                logger.LogInformation(message(traceId));
        }
        #endregion

        #region Async
        /// <summary>
        /// 结束跟踪
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="logger">日志</param>
        /// <param name="function">执行方法名称</param>
        /// <param name="traceKey">跟踪请求头部key</param>
        /// <param name="writeTraceId">是否写入traceId到响应headers中</param>
        public static async Task EndTraceAsync(this ServiceContext @this, ILogger logger, string function, string traceKey = "traceid", bool writeTraceId = true)
        {
            await @this.TraceAsync(logger, async x =>
            {
                if (writeTraceId)
                    await @this.WriteHeadersAsync(x);

                return $"traceId -> {x} -> `End {function}`.";

            }, traceKey);
        }

        /// <summary>
        /// 日志跟踪
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="logger">日志</param>
        /// <param name="message">自定义跟踪消息委托，参数TraceId</param>
        /// <param name="traceKey">跟踪请求头部key</param>
        public static async Task TraceAsync(this ServiceContext @this, ILogger logger, Func<string, Task<string>> message, string traceKey = "traceid")
        {
            var traceId = @this.GetTraceId(traceKey);

            if (message.IsNotNull() && traceId.IsNotNullOrEmpty())
                logger.LogInformation(await message(traceId));
        }
        #endregion
        #endregion

        #region WithHeaders
        /// <summary>
        /// gRPC客户端添加Headers
        /// </summary>
        /// <typeparam name="TService">gRPC服务</typeparam>
        /// <param name="this"><see cref="IService{TService}"/></param>
        /// <param name="traceId">要跟踪记录的traceId</param>
        /// <param name="traceKey">headers中traceId对应的key</param>
        /// <returns></returns>
        public static TService WithHeaders<TService>(this IService<TService> @this, object traceId, string traceKey = "traceid")
        {
            var headers = new Metadata();

            if (traceKey.IsNotNullOrEmpty() && traceId.IsNotNull())
                headers.Add(traceKey, traceId.ToString());

            return @this.WithHeaders(headers);
        }

        /// <summary>
        /// gRPC客户端添加Headers，注意：key为小写或者形如：user-agent
        /// </summary>
        /// <typeparam name="TService">gRPC服务</typeparam>
        /// <param name="this"><see cref="IService{TService}"/></param>
        /// <param name="headers">gRPC客户端请求headers</param>
        /// <returns></returns>
        public static TService WithHeaders<TService>(this IService<TService> @this, Dictionary<string, object> headers)
        {
            var metadata = new Metadata();

            if (headers.IsNotNullOrEmpty())
            {
                foreach (var header in headers)
                {
                    if (header.Key.IsNotNullOrEmpty() && header.Value.IsNotNull())
                        metadata.Add(header.Key, header.Value.ToString());
                }
            }

            return @this.WithHeaders(metadata);
        }

        /// <summary>
        /// gRPC客户端添加Headers，注意：key为小写或者形如：user-agent
        /// </summary>
        /// <typeparam name="TService">gRPC服务</typeparam>
        /// <param name="this"><see cref="IService{TService}"/></param>
        /// <param name="headers">gRPC客户端请求headers</param>
        /// <returns></returns>
        public static TService WithHeaders<TService>(this IService<TService> @this, params (string key, object value)[] headers)
        {
            var metadata = new Metadata();

            if (headers.IsNotNullOrEmpty())
            {
                foreach (var (key, value) in headers)
                {
                    if (key.IsNotNullOrEmpty() && value.IsNotNull())
                        metadata.Add(key, value.ToString());
                }
            }

            return @this.WithHeaders(metadata);
        }
        #endregion

        #region WriteHeaders
        #region Sync
        /// <summary>
        /// gRPC服务端添加请求响应Headers
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="traceId">要跟踪记录的traceId</param>
        /// <param name="traceKey">headers中traceId对应的key</param>
        /// <returns></returns>
        public static void WriteHeaders(this ServiceContext @this, object traceId, string traceKey = "traceid")
        {
            if (@this.IsNotNull() && traceId.IsNotNull() && traceKey.IsNotNullOrEmpty())
                @this.CallContext.GetHttpContext().Response.Headers.Add(traceKey, traceId.ToString());
        }

        /// <summary>
        /// gRPC服务端添加请求响应Headers，注意：key为小写或者形如：user-agent
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="headers">gRPC客户端请求headers</param>
        /// <returns></returns>
        public static void WriteHeaders(this ServiceContext @this, Dictionary<string, object> headers)
        {
            if (@this.IsNotNull() && headers.IsNotNullOrEmpty())
            {
                var responseHeaders = @this.CallContext.GetHttpContext().Response.Headers;

                foreach (var header in headers)
                {
                    if (header.Key.IsNotNullOrEmpty() && header.Value.IsNotNull())
                        responseHeaders.Add(header.Key, header.Value.ToString());
                }
            }
        }

        /// <summary>
        /// gRPC服务端添加请求响应Headers，注意：key为小写或者形如：user-agent
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="headers">gRPC客户端请求headers</param>
        /// <returns></returns>
        public static void WriteHeaders(this ServiceContext @this, params (string key, object value)[] headers)
        {
            if (@this.IsNotNull() && headers.IsNotNullOrEmpty())
            {
                var responseHeaders = @this.CallContext.GetHttpContext().Response.Headers;

                foreach (var (key, value) in headers)
                {
                    if (key.IsNotNullOrEmpty() && value.IsNotNull())
                        responseHeaders.Add(key, value.ToString());
                }
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// gRPC服务端添加请求响应Headers
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="headers">gRPC服务器端响应头部数据</param>
        /// <returns></returns>
        public static async Task WriteHeadersAsync(this ServiceContext @this, Metadata headers)
        {
            if (@this.IsNotNull() && headers.IsNotNull())
                await @this.CallContext.WriteResponseHeadersAsync(headers);
        }

        /// <summary>
        /// gRPC服务端添加请求响应Headers
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="traceId">要跟踪记录的traceId</param>
        /// <param name="traceKey">headers中traceId对应的key</param>
        /// <returns></returns>
        public static async Task WriteHeadersAsync(this ServiceContext @this, object traceId, string traceKey = "traceid")
        {
            if (traceKey.IsNotNullOrEmpty() && traceId.IsNotNull())
                await @this.WriteHeadersAsync(
                    new Metadata
                    {
                        { traceKey, traceId.ToString() }
                    });
        }

        /// <summary>
        /// gRPC服务端添加请求响应Headers，注意：key为小写或者形如：user-agent
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="headers">gRPC客户端请求headers</param>
        /// <returns></returns>
        public static async Task WriteHeadersAsync(this ServiceContext @this, Dictionary<string, object> headers)
        {
            if (@this.IsNotNull() && headers.IsNotNullOrEmpty())
            {
                var metadata = new Metadata();

                foreach (var header in headers)
                {
                    if (header.Key.IsNotNullOrEmpty() && header.Value.IsNotNull())
                        metadata.Add(header.Key, header.Value.ToString());
                }

                await @this.CallContext.WriteResponseHeadersAsync(metadata);
            }
        }

        /// <summary>
        /// gRPC服务端添加请求响应Headers，注意：key为小写或者形如：user-agent
        /// </summary>
        /// <param name="this"><see cref="ServiceContext"/></param>
        /// <param name="headers">gRPC客户端请求headers</param>
        /// <returns></returns>
        public static async Task WriteHeadersAsync(this ServiceContext @this, params (string key, object value)[] headers)
        {
            if (@this.IsNotNull() && headers.IsNotNullOrEmpty())
            {
                var metadata = new Metadata();

                foreach (var (key, value) in headers)
                {
                    if (key.IsNotNullOrEmpty() && value.IsNotNull())
                        metadata.Add(key, value.ToString());
                }

                await @this.CallContext.WriteResponseHeadersAsync(metadata);
            }
        }
        #endregion
        #endregion
    }
}
