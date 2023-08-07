using System;

namespace Bi.Core.Attributes
{
    /// <summary>
    /// Swagger忽略输入和输出参数特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SwaggerIgnoreAttribute : Attribute
    {
    }
}
