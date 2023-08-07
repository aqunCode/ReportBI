using System;

namespace Bi.Core.Caches
{
    /// <summary>
    /// StackExchangeRedis会话信息
    /// </summary>
    public class RedisProfilingSession
    {
        /// <summary>
        /// redis数据库
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// redis命令
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// redis服务器地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// redis服务器端口号
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// redis命令创建时间
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// redis命令耗时，单位ms
        /// </summary>
        public double ElapsedMilliseconds { get; set; }
    }
}
