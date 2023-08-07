using Bi.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bi.Core.Models
{
    /// <summary>
    /// 基类
    /// </summary>
    //[Authorize]
    [ApiController] //ApiController特性不添加则接收json格式参数需要添加FromBody特性
    public abstract class BaseController : ControllerBase
    {
        #region 字段
        private readonly long _timestamp;

        private readonly ITranslateService _translateService;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// <para>
        /// 注意：若要翻译错误码，则需要StartUp继承DefaultStartup，或者在StartUp的Configure中设置：App.ServiceProvider = app.ApplicationServices;
        /// </para>
        /// </summary>
        public BaseController()
        {
            _timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _translateService = App.GetService<ITranslateService>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="translateService"></param>
        public BaseController(ITranslateService translateService)
        {
            _timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _translateService = translateService;
        }
        #endregion

        #region 属性
        /// <summary>
        /// 当前请求头部Token
        /// </summary>
        public string Authorization => this.HttpContext.Request.Headers["Authorization"];

        /// <summary>
        /// 当前操作者
        /// </summary>
        /// <returns></returns>
        public CurrentUser CurrentUser => Operator.GetUserFromToken(this.Authorization);
        #endregion

        #region 成功
        /// <summary>
        /// 成功
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public ResponseResult Success() =>
            new ResponseResult(ResponseCode.Ok).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult Success(string message) =>
            new ResponseResult(ResponseCode.Ok, message).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 成功
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="data">成功数据</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult<T> Success<T>(T data) =>
            new ResponseResult<T>(ResponseCode.Ok, data).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 成功
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="message">成功消息</param>
        /// <param name="data">成功数据</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult<T> Success<T>(string message, T data) =>
            new ResponseResult<T>(ResponseCode.Ok, message, data).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 成功
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="data">成功分页数据</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult<PageEntity<T>> Success<T>(PageEntity<T> data) =>
            new ResponseResult<PageEntity<T>>(ResponseCode.Ok, data).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 成功
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="message">成功消息</param>
        /// <param name="data">成功分页数据</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult<PageEntity<T>> Success<T>(string message, PageEntity<T> data) =>
            new ResponseResult<PageEntity<T>>(ResponseCode.Ok, message, data).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);
        #endregion

        #region 错误
        /// <summary>
        /// 错误
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public ResponseResult Error() =>
            new ResponseResult(ResponseCode.Error).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="code">响应码</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult Error(ResponseCode code) =>
            new ResponseResult(code).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="args">错误码对应的占位符参数</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult Error(double errorCode, params object[] args) =>
            new ResponseResult
            {
                Code = ResponseCode.Error,
                ErrorCode = errorCode,
                Message = _translateService?.TranslateErrorCode(errorCode, args)
            }.SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="args">错误码对应的占位符参数</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult Error(string message, double errorCode, params object[] args) =>
            new ResponseResult
            {
                Message = _translateService == null ? message : $"{message}({_translateService.TranslateErrorCode(errorCode, args)})",
                Code = ResponseCode.Error,
                ErrorCode = errorCode
            }.SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult Error(string message) =>
            new ResponseResult(ResponseCode.Error, message).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 错误
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="data">错误数据</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult<T> Error<T>(T data) =>
            new ResponseResult<T>(ResponseCode.Error, data).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 错误
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="message">错误消息</param>
        /// <param name="data">错误数据</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult<T> Error<T>(string message, T data) =>
            new ResponseResult<T>(ResponseCode.Error, message, data).
                SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 错误
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="errorCode">错误码</param>
        /// <param name="data">错误数据</param>
        /// <param name="args">错误码对应的占位符参数</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult<T> Error<T>(double errorCode, T data, params object[] args) =>
            new ResponseResult<T>(ResponseCode.Error, data)
            {
                ErrorCode = errorCode,
                Message = _translateService?.TranslateErrorCode(errorCode, args)
            }.SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);

        /// <summary>
        /// 错误
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="message">错误消息</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="data">错误数据</param>
        /// <param name="args">错误码对应的占位符参数</param>
        /// <returns></returns>
        [NonAction]
        public ResponseResult<T> Error<T>(string message, double errorCode, T data, params object[] args) =>
            new ResponseResult<T>(ResponseCode.Error, message, data)
            {
                ErrorCode = errorCode,
                Message = _translateService == null ? message : $"{message}({_translateService.TranslateErrorCode(errorCode, args)})",
            }.SetElapsedMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._timestamp);
        #endregion
    }
}
