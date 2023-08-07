using System;

namespace Bi.Core.Models
{
    /// <summary>
    /// 通用响应实体
    /// </summary>
    public class ObjectResponse
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? ObjectTime { get; set; }
    }
}
