using Bi.Core.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Bi.Core.Attributes
{
    /// <summary>
    /// Range[String版本]
    /// String类型的
    /// 参数校验
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RangeStringAttribute : ValidationAttribute
    {
        /// <summary>
        /// 字符串集合
        /// </summary>
        private readonly string[] _rangeArray;

        /// <summary>
        /// 是否不区分大小写，默认区分大小写
        /// </summary>
        public bool IgnoreCase { get; set; } = false;

        /// <summary>
        /// 错误提示格式化字符串
        /// </summary>
        public string ErrorFormatMsg { get; set; } = "参数:{0}:必须要在有效范围:[{1}]";

        ///<summary>
        /// 构造函数
        /// </summary>
        /// <param name="rangeArray">合法有效的字符串范围</param>
        public RangeStringAttribute(params string[] rangeArray)
        {
            _rangeArray = rangeArray;
        }

        /// <summary>
        /// 格式化错误消息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorFormatMsg, name, _rangeArray.Join("],["));
        }

        /// <summary>
        /// 校验
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            if (_rangeArray.Any(x => x.Equals(IgnoreCase, value?.ToString())))
                return true;

            return false;
        }
    }
}
