using System;
using System.Linq;
using System.Reflection;

namespace Bi.Core.Extensions
{
    /// <summary>
    /// Exception扩展类
    /// </summary>
    public static class ExceptionExtensions
    {
        #region 获取内部真实异常
        /// <summary>
        /// 获取内部真实异常
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Exception GetTrue(this Exception @this)
        {
            if (@this == null) return null;

            if (@this is AggregateException)
                return GetTrue((@this as AggregateException).Flatten().InnerException);

            if (@this is TargetInvocationException)
                return GetTrue((@this as TargetInvocationException).InnerException);

            if (@this is TypeInitializationException)
                return GetTrue((@this as TypeInitializationException).InnerException);

            return @this.GetBaseException() ?? @this;
        }

        /// <summary>
        /// 获取内部异常
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception GetInnerException(this Exception ex)
        {
            Exception innerException = ex.InnerException;
            Exception result = ex;
            while (innerException != null)
            {
                result = innerException;
                innerException = innerException.InnerException;
            }
            return result;
        }
        #endregion

        #region 获取异常消息
        /// <summary>
        /// 获取异常消息
        /// </summary>
        /// <param name="this">异常</param>
        /// <returns></returns>
        public static string GetMessage(this Exception @this)
        {
            var msg = @this + "";
            if (msg.IsNullOrEmpty()) return null;

            var ss = msg.Split(Environment.NewLine);
            var ns = ss.Where(e =>
            !e.StartsWith("---") &&
            !e.Contains("System.Runtime.ExceptionServices") &&
            !e.Contains("System.Runtime.CompilerServices"));

            msg = ns.Join(Environment.NewLine);

            return msg;
        }
        #endregion
    }
}
