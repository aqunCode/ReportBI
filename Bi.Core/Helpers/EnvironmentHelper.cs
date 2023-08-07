using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bi.Core.Extensions;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// 环境变量工具类
    /// </summary>
    public class EnvironmentHelper
    {
        /// <summary>
        /// 获取环境变量
        /// </summary>
        /// <param name="value">含有环境变量的组合字符串，eg:${LogPath}|NLog.config</param>
        /// <returns>string</returns>
        public static string GetEnvironmentVariable(string value)
        {
            var result = value;
            var param = GetParameters(result).FirstOrDefault();
            if (param.IsNotNullOrEmpty())
            {
                var env = Environment.GetEnvironmentVariable(param);
                result = env;
                if (string.IsNullOrEmpty(env))
                {
                    var arrayData = value.ToString().Split('|');
                    result = arrayData.Length == 2 ? arrayData[1] : env;
                }
            }
            else
            {
                result = Environment.GetEnvironmentVariable(value);
            }

            return result;
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="text">参数字符串，eg:${LogPath}|NLog.config</param>
        /// <returns>string</returns>
        private static List<string> GetParameters(string text)
        {
            var matchVales = new List<string>();
            var pattern = @"(?<=\${)[^\${}]*(?=})";
            foreach (Match item in Regex.Matches(text, pattern))
            {
                matchVales.Add(item.Value);
            }
            return matchVales;
        }
    }
}
