using Bi.Core.Attributes;
using MessagePack;
using System.ComponentModel.DataAnnotations;

namespace Bi.Core.Models
{
    /// <summary>
    /// api接口输入参数抽象基类
    /// </summary>
    [MessagePackObject(true)]
    public abstract class BaseInput : BaseCapMessage
    {
        /// <summary>
        /// 当前操作者
        /// </summary>
        [SwaggerIgnore]
        public CurrentUser CurrentUser { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int? SortCode { get; set; }

        /// <summary>
        /// 是否有效 -1所有 0-无效 1-有效
        /// </summary>
        [Required]
        [Range(-1, 1)]
        public int Enabled { get; set; } = 1;

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// api接口输入参数抽象基类
    /// </summary>
    [MessagePackObject(true)]
    public abstract class NewBaseInput
    {
        /// <summary>
        /// 当前操作者
        /// </summary>
        [SwaggerIgnore]
        public CurrentUser CurrentUser { get; set; }
    }
}
