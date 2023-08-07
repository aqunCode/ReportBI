using System;
using Microsoft.AspNetCore.Http;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// Cookie工具类
    /// </summary>
    public class CookieHelper
    {
        #region 写入cookie
        /// <summary>
        ///  写入cookie
        /// </summary>
        /// <param name="strName">cookie名称</param>
        /// <param name="strValue">cookie值</param>
        public static void Set(string strName, string strValue)
        {
            var cookie = HttpContextHelper.Current.Request.Cookies[strName];
            if (cookie == null)
            {
                HttpContextHelper.Current.Response.Cookies.Append(strName, strValue);
            }
        }

        /// <summary>
        /// 写入cookie
        /// </summary>
        /// <param name="strName">cookie名称</param>
        /// <param name="strValue">cookie值</param>
        /// <param name="expires">过期时间(单位：分钟)</param>
        public static void Set(string strName, string strValue, int expires)
        {
            var cookie = HttpContextHelper.Current.Request.Cookies[strName];
            if (cookie == null)
            {
                HttpContextHelper.Current.Response.Cookies.Append(strName, strValue, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddMinutes(expires)
                });
            }
        }
        #endregion

        #region 读取cookie
        /// <summary>
        /// 获取cookie
        /// </summary>
        /// <param name="strName">cookie名称</param>
        /// <returns>string</returns>
        public static string Get(string strName)
        {
            return HttpContextHelper.Current.Request.Cookies?[strName];
        }
        #endregion
    }
}
