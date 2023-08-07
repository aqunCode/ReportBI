using System;

namespace Bi.Core.Exceptions
{
    /// <summary>
    /// 自定义接口提示异常
    /// </summary>
    public class TipsException : Exception
    {
        /// <summary>
        /// 自定义异常
        /// </summary>
        /// <param name="message">自定义异常消息/错误码</param>
        public TipsException(string message) : base(message) { }

        /// <summary>
        /// 自定义异常
        /// </summary>
        /// <param name="errorCode">错误码</param>
        public TipsException(double errorCode) : this(errorCode.ToString()) { }

        /// <summary>
        /// 自定义异常
        /// </summary>
        /// <param name="errorCode">错误码</param>
        public TipsException(int errorCode) : this(errorCode.ToString()) { }
    }
}
