﻿using Bi.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// HttpClient工具类
    /// </summary>
    public static class HttpClientHelper
    {
        #region Public Static Property
        /// <summary>
        /// HttpClientFactory工厂
        /// </summary>
        public static IHttpClientFactory HttpClientFactory { get; set; } = null;

        /// <summary>
        /// UserAgent
        /// </summary>
        public static string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
        #endregion

        #region Extension Method
        /// <summary>
        /// 将Http状态码翻译为对应的中文
        /// </summary>
        /// <param name="code">Http状态码</param>
        /// <returns>中文解析</returns>
        public static string ToChsText(this HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.Continue:
                    return "请求者应继续进行请求";
                case HttpStatusCode.SwitchingProtocols:
                    return "请求者已要求服务器切换协议，服务器已确认并准备进行切换";
                case HttpStatusCode.OK:
                    return "服务器成功处理了相应请求";
                case HttpStatusCode.Created:
                    return "请求成功且服务器已创建了新的资源";
                case HttpStatusCode.Accepted:
                    return "服务器已接受相应请求，但尚未对其进行处理";
                case HttpStatusCode.NonAuthoritativeInformation:
                    return "服务器已成功处理相应请求，但返回了可能来自另一来源的信息";
                case HttpStatusCode.NoContent:
                    return "服务器已成功处理相应请求，但未返回任何内容";
                case HttpStatusCode.ResetContent:
                    return "服务器已成功处理相应请求，但未返回任何内容，但要求请求者重置文档视图";
                case HttpStatusCode.PartialContent:
                    return "服务器成功处理了部分 GET 请求";
                case HttpStatusCode.MultipleChoices:
                    return "服务器可以根据请求来执行多项操作";
                case HttpStatusCode.Moved:
                    return "请求的网页已永久移动到新位置";
                case HttpStatusCode.Redirect:
                    return "服务器目前正从不同位置的网页响应请求，但请求者应继续使用原有位置来进行以后的请求";
                case HttpStatusCode.RedirectMethod:
                    return "当请求者应对不同的位置进行单独的 GET 请求以检索响应时，服务器会返回此代码";
                case HttpStatusCode.NotModified:
                    return "请求的网页自上次请求后再也没有修改过";
                case HttpStatusCode.UseProxy:
                    return "请求者只能使用代理访问请求的网页";
                case HttpStatusCode.Unused:
                    return "Unused 是未完全指定的 HTTP/1.1 规范的建议扩展";
                case HttpStatusCode.RedirectKeepVerb:
                    return "服务器目前正从不同位置的网页响应请求，但请求者应继续使用原有位置来进行以后的请求";
                case HttpStatusCode.BadRequest:
                    return "服务器未能识别请求";
                case HttpStatusCode.Unauthorized:
                    return "请求要求进行身份验证";
                case HttpStatusCode.PaymentRequired:
                    return "保留 PaymentRequired 以供将来使用";
                case HttpStatusCode.Forbidden:
                    return "服务器拒绝相应请求";
                case HttpStatusCode.NotFound:
                    return "服务器找不到请求的资源";
                case HttpStatusCode.MethodNotAllowed:
                    return "禁用相应请求中所指定的方法";
                case HttpStatusCode.NotAcceptable:
                    return "无法使用相应请求的内容特性来响应请求的网页";
                case HttpStatusCode.ProxyAuthenticationRequired:
                    return "请求者应当使用代理进行授权";
                case HttpStatusCode.RequestTimeout:
                    return "服务器在等待请求时超时";
                case HttpStatusCode.Conflict:
                    return "服务器在完成请求时遇到冲突";
                case HttpStatusCode.Gone:
                    return "请求的资源已被永久删除";
                case HttpStatusCode.LengthRequired:
                    return "服务器不会接受包含无效内容长度标头字段的请求";
                case HttpStatusCode.PreconditionFailed:
                    return "服务器未满足请求者在请求中设置的其中一个前提条件";
                case HttpStatusCode.RequestEntityTooLarge:
                    return "服务器无法处理相应请求，因为请求实体过大，已超出服务器的处理能力";
                case HttpStatusCode.RequestUriTooLong:
                    return "请求的 URI 过长，服务器无法进行处理";
                case HttpStatusCode.UnsupportedMediaType:
                    return "相应请求的格式不受请求页面的支持";
                case HttpStatusCode.RequestedRangeNotSatisfiable:
                    return "如果相应请求是针对网页的无效范围进行的，那么服务器会返回此状态代码";
                case HttpStatusCode.ExpectationFailed:
                    return "服务器未满足“期望”请求标头字段的要求";
                case HttpStatusCode.InternalServerError:
                    return "服务器内部遇到错误，无法完成相应请求";
                case HttpStatusCode.NotImplemented:
                    return "请求的功能在服务器中尚未实现";
                case HttpStatusCode.BadGateway:
                    return "服务器作为网关或代理，从上游服务器收到了无效的响应";
                case HttpStatusCode.ServiceUnavailable:
                    return "目前服务器不可用（由于超载或进行停机维护）";
                case HttpStatusCode.GatewayTimeout:
                    return "服务器作为网关或代理，未及时从上游服务器接收请求";
                case HttpStatusCode.HttpVersionNotSupported:
                    return "服务器不支持相应请求中所用的 HTTP 协议版本";
                default:
                    return "未知Http状态";
            }
        }
        #endregion

        #region UseHttpClientFactory
        /// <summary>
        /// 使用HttpClient工厂模式
        /// </summary>
        /// <param name="app"></param>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services)
        ///         {
        ///             services.AddHttpClient();
        ///         }
        ///         
        ///         public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider svp)
        ///         {
        ///             //配置UseHttpClientFactory
        ///             app.UseHttpClientFactory();
        ///         }
        ///     </code>
        /// </example>
        public static void UseHttpClientFactory(this IApplicationBuilder app)
        {
            HttpClientFactory = app.ApplicationServices.GetRequiredService<IHttpClientFactory>();
        }

        /// <summary>
        /// 使用HttpClient工厂模式
        /// </summary>
        /// <param name="provider"></param>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services)
        ///         {
        ///             services.AddHttpClient();
        ///         }
        ///         
        ///         public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider svp)
        ///         {
        ///             //配置UseHttpClientFactory
        ///             svp.UseHttpClientFactory();
        ///         }
        ///     </code>
        /// </example>
        public static void UseHttpClientFactory(this IServiceProvider provider)
        {
            HttpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        }
        #endregion

        #region CreateHttpClient
        /// <summary>
        /// 创建HttpClient
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="httpClientAction">HttpClient自定义委托</param>
        /// <returns>返回HttpClient</returns>
        public static HttpClient CreateHttpClient(
            string url,
            string httpClientName = null,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> httpClientAction = null)
        {
            HttpClient httpClient = null;
            if (HttpClientFactory != null)
            {
                if (httpClientName.IsNotNullOrEmpty())
                    httpClient = HttpClientFactory.CreateClient(httpClientName);
                else
                    httpClient = HttpClientFactory.CreateClient();
            }
            else
            {
                var handler = new HttpClientHandler { AutomaticDecompression = decompressionMethods };
                if (url.StartsWithIgnoreCase("https"))
                    handler.ServerCertificateCustomValidationCallback = (request, certificate, chain, errors) => true;

                httpClient = new HttpClient(handler);
            }

            httpClient.CancelPendingRequests();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("user-agent", UserAgent);

            if (!accept.IsNullOrEmpty())
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept) { CharSet = "utf-8" });

            if (headers?.Count > 0)
            {
                foreach (var item in headers)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                }
            }

            //自定义委托处理HttpClient
            httpClientAction?.Invoke(httpClient);

            return httpClient;
        }
        #endregion

        #region GetAsync
        /// <summary>
        /// 根据Url地址Get请求返回数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">请求参数</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="delegate">自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(string result, HttpStatusCode code)> GetAsync(
            string url,
            Dictionary<string, string> parameters,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> @delegate = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, @delegate);

            (string result, HttpStatusCode code) result;
            using (var response = await httpClient.GetAsync(url + parameters.ToUrl("?", false, false)))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = (await response.Content.ReadAsStringAsync(), httpStatusCode);
                else
                    result = (null, httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }

        /// <summary>
        /// 根据Url地址Get请求返回实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">请求参数</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="delegate">自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(T result, HttpStatusCode code)> GetAsync<T>(
            string url,
            Dictionary<string, string> parameters,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> @delegate = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, @delegate);

            (T result, HttpStatusCode code) result;
            using (var response = await httpClient.GetAsync(url + parameters.ToUrl("?", false, false)))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = ((await response.Content.ReadAsStringAsync()).ToObject<T>(), httpStatusCode);
                else
                    result = (default(T), httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }
        #endregion

        #region PostAsync
        /// <summary>
        /// Post请求返回字符
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="content">请求内容</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="delegate">自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(string result, HttpStatusCode code)> PostAsync(
            string url,
            HttpContent content,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> @delegate = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, @delegate);

            (string result, HttpStatusCode code) result;
            using (var response = await httpClient.PostAsync(url, content))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = (await response.Content.ReadAsStringAsync(), httpStatusCode);
                else
                    result = (null, httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }

        /// <summary>
        /// Post请求返回字符
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="contentType">客户端发送的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="delegate">自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(string result, HttpStatusCode code)> PostAsync(
            string url,
            object data,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            string contentType = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> @delegate = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, @delegate);

            var content = new StringContent((data?.GetType() == typeof(string) ? data?.ToString() : data?.ToJson()) ?? "", Encoding.UTF8);
            if (!contentType.IsNullOrEmpty())
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType) { CharSet = "utf-8" };

            (string result, HttpStatusCode code) result;
            using (var response = await httpClient.PostAsync(url, content))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = (await response.Content.ReadAsStringAsync(), httpStatusCode);
                else
                    result = (null, httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }

        /// <summary>
        /// Post请求返回实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="content">请求内容</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="delegate">自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(T result, HttpStatusCode code)> PostAsync<T>(
            string url,
            HttpContent content,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> @delegate = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, @delegate);

            (T result, HttpStatusCode code) result;
            using (var response = await httpClient.PostAsync(url, content))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = ((await response.Content.ReadAsStringAsync()).ToObject<T>(), httpStatusCode);
                else
                    result = (default(T), httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }

        /// <summary>
        /// Post请求返回实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="contentType">客户端发送的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="delegate">自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(T result, HttpStatusCode code)> PostAsync<T>(
            string url,
            object data,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            string contentType = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> @delegate = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, @delegate);

            var content = new StringContent((data?.GetType() == typeof(string) ? data?.ToString() : data?.ToJson()) ?? "", Encoding.UTF8);
            if (!contentType.IsNullOrEmpty())
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType) { CharSet = "utf-8" };

            (T result, HttpStatusCode code) result;
            using (var response = await httpClient.PostAsync(url, content))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = ((await response.Content.ReadAsStringAsync()).ToObject<T>(), httpStatusCode);
                else
                    result = (default(T), httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }
        #endregion

        #region SendAsync
        /// <summary>
        /// HttpClient SendAsync方式请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="content">请求内容</param>
        /// <param name="method">请求方式</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="httpClientAction">HttpClient自定义委托</param>
        /// <param name="httpRequestMessageAction">HttpRequestMessage自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(string result, HttpStatusCode code)> SendAsync(
            string url,
            HttpContent content,
            HttpMethod method,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> httpClientAction = null,
            Action<HttpRequestMessage> httpRequestMessageAction = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, httpClientAction);

            var req = new HttpRequestMessage(method, url);
            if (content != null)
                req.Content = content;

            httpRequestMessageAction?.Invoke(req);

            (string result, HttpStatusCode code) result;
            using (var response = await httpClient.SendAsync(req))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = (await response.Content.ReadAsStringAsync(), httpStatusCode);
                else
                    result = (null, httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }

        /// <summary>
        /// HttpClient SendAsync方式请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求参数</param>
        /// <param name="method">请求方式</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="contentType">客户端发送的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="httpClientAction">HttpClient自定义委托</param>
        /// <param name="httpRequestMessageAction">HttpRequestMessage自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(string result, HttpStatusCode code)> SendAsync(
            string url,
            object data,
            HttpMethod method,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            string contentType = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> httpClientAction = null,
            Action<HttpRequestMessage> httpRequestMessageAction = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, httpClientAction);

            var req = new HttpRequestMessage(method, url);
            if (data != null)
                req.Content = new StringContent((data?.GetType() == typeof(string) ? data?.ToString() : data?.ToJson()) ?? "", Encoding.UTF8);

            if (!contentType.IsNullOrEmpty() && req.Content != null)
                req.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType) { CharSet = "utf-8" };

            httpRequestMessageAction?.Invoke(req);

            (string result, HttpStatusCode code) result;
            using (var response = await httpClient.SendAsync(req))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = (await response.Content.ReadAsStringAsync(), httpStatusCode);
                else
                    result = (null, httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }

        /// <summary>
        ///  HttpClient SendAsync方式请求
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="content">请求内容</param>
        /// <param name="method">请求方式</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="httpClientAction">HttpClient自定义委托</param>
        /// <param name="httpRequestMessageAction">HttpRequestMessage自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(T result, HttpStatusCode code)> SendAsync<T>(
            string url,
            HttpContent content,
            HttpMethod method,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> httpClientAction = null,
            Action<HttpRequestMessage> httpRequestMessageAction = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, httpClientAction);

            var req = new HttpRequestMessage(method, url);
            if (content != null)
                req.Content = content;

            httpRequestMessageAction?.Invoke(req);

            (T result, HttpStatusCode code) result;
            using (var response = await httpClient.SendAsync(req))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = ((await response.Content.ReadAsStringAsync()).ToObject<T>(), httpStatusCode);
                else
                    result = (default(T), httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }

        /// <summary>
        ///  HttpClient SendAsync方式请求
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求参数</param>
        /// <param name="method">请求方式</param>
        /// <param name="decompressionMethods">解压缩方式，默认：GZip</param>
        /// <param name="accept">客户端希望接收的数据类型</param>
        /// <param name="contentType">客户端发送的数据类型</param>
        /// <param name="headers">头部信息</param>
        /// <param name="httpClientAction">HttpClient自定义委托</param>
        /// <param name="httpRequestMessageAction">HttpRequestMessage自定义委托</param>
        /// <param name="httpClientName">HttpClient注入的名称，此名称用于HttpClientFactory创建HttpClient使用</param>
        /// <returns>返回请求结果和状态结果</returns>
        public static async Task<(T result, HttpStatusCode code)> SendAsync<T>(
            string url,
            object data,
            HttpMethod method,
            DecompressionMethods decompressionMethods = DecompressionMethods.GZip,
            string accept = "application/json",
            string contentType = "application/json",
            Dictionary<string, string> headers = null,
            Action<HttpClient> httpClientAction = null,
            Action<HttpRequestMessage> httpRequestMessageAction = null,
            string httpClientName = null)
        {
            var httpClient = CreateHttpClient(url, httpClientName, decompressionMethods, accept, headers, httpClientAction);

            var req = new HttpRequestMessage(method, url);
            if (data != null)
                req.Content = new StringContent((data?.GetType() == typeof(string) ? data?.ToString() : data?.ToJson()) ?? "", Encoding.UTF8);

            if (!contentType.IsNullOrEmpty() && req.Content != null)
                req.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType) { CharSet = "utf-8" };

            httpRequestMessageAction?.Invoke(req);

            (T result, HttpStatusCode code) result;
            using (var response = await httpClient.SendAsync(req))
            {
                var httpStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                    result = ((await response.Content.ReadAsStringAsync()).ToObject<T>(), httpStatusCode);
                else
                    result = (default(T), httpStatusCode);
            }

            //判断是否是IHttpClientFactory工厂创建，若不是则释放HttpClient
            if (HttpClientFactory == null)
                httpClient.Dispose();

            return result;
        }
        #endregion
    }
}
