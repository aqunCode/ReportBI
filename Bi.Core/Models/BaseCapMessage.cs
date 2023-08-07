using Bi.Core.Attributes;
using Bi.Core.Helpers;
using MessagePack;
using System;

namespace Bi.Core.Models
{
    /// <summary>
    /// Cap消息基类
    /// </summary>
    [MessagePackObject(true)]
    public abstract class BaseCapMessage
    {
        /// <summary>
        /// Cap消息唯一标识Id
        /// </summary>
        [SwaggerIgnore]
        public string MessageId { get; set; } = Sys.Guid;
    }
}
