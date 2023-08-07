using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bi.Core.Models
{
    using MessagePack;

    /// <summary>
    /// 请求响应状态码
    /// </summary>
    public enum ResponseCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")] Ok = 200,

        /// <summary>
        /// 失败
        /// </summary>
        [Description("失败")] Error = -1,

        /// <summary>
        /// 未授权
        /// </summary>
        [Description("未授权")] Unauthorized = 401,

        /// <summary>
        /// 服务器异常
        /// </summary>
        [Description("服务器异常")] InternalServerError = 500
    }

    /// <summary>
    /// 通用返回结果
    /// </summary>
    [MessagePackObject(true)]
    public class ResponseResult<T>
    {
        /// <summary>
        /// 空构造函数
        /// </summary>
        public ResponseResult() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ResponseResult(ResponseCode code)
        {
            this.Code = code;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="result"></param>
        public ResponseResult(ResponseCode code, T result)
        {
            this.Code = code;
            this.Result = result;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ResponseResult(ResponseCode code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public ResponseResult(ResponseCode code, string message, T result)
        {
            this.Code = code;
            this.Message = message;
            this.Result = result;
        }

        /// <summary>
        /// 设置接口耗时
        /// </summary>
        /// <param name="elapsedMilliseconds"></param>
        /// <returns></returns>
        public virtual ResponseResult<T> SetElapsedMilliseconds(long elapsedMilliseconds)
        {
            this.ElapsedMilliseconds = elapsedMilliseconds;

            return this;
        }

        /// <summary>
        /// 返回状态码
        /// </summary>
        public ResponseCode Code { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public double ErrorCode { get; set; } = -1;

        /// <summary>
        /// 是否分页
        /// </summary>
        public bool HasPage { get; set; } = false;

        /// <summary>
        /// 接口返回消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 接口耗时(ms)
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 接口返回具体结果
        /// </summary>
        public T Result { get; set; }
    }

    /// <summary>
    /// 通用返回结果
    /// </summary>
    [MessagePackObject(true)]
    public class ResponseResult : ResponseResult<string>
    {
        public ResponseResult() : base() { }
        public ResponseResult(ResponseCode code) : base(code) { }
        public ResponseResult(ResponseCode code, string message) : base(code, message) { }
        public ResponseResult(ResponseCode code, string message, string data) : base(code, message, data) { }

        public override ResponseResult SetElapsedMilliseconds(long elapsedMilliseconds)
        {
            this.ElapsedMilliseconds = elapsedMilliseconds;

            return this;
        }
    }

    /// <summary>
    /// 分页实体
    /// </summary>
    [MessagePackObject(true)]
    public class PageEntity<T>
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        [Required]
        public string OrderField { get; set; }

        /// <summary>
        /// 是否升序
        /// </summary>
        [Required]
        public bool Ascending { get; set; }

        /// <summary>
        /// 分页大小
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = 30;

        /// <summary>
        /// 当前页码
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int PageIndex { get; set; }

        /// <summary>
        /// 总条数，不需要填写
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// 总页数，不需要填写
        /// </summary>
        public int TotalPage => (int)(Total / (PageSize == 0 ? 30 : PageSize) + (Total % (PageSize == 0 ? 30 : PageSize) == 0 ? 0 : 1));

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }
    }
}
