using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Bi.Core.Attributes
{
    /// <summary>
    /// Controller中的方法参数或者属性注入特性，注意：只适用于Controller中，且构造函数中该属性为null，不可直接使用，若要使用则需要采用构造函数方式注入
    /// </summary>
    /// <remarks>
    ///     <code>
    ///         1. [FromServices] public IUserService UserService { get; set; }
    ///         2. public async Task&lt;ActionResult&lt;Account&gt;&gt; ThrowException(Transaction transaction, [FromServices] DaprClient daprClient)
    ///     </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromServiceAttribute : Attribute, IBindingSourceMetadata
    {
        /// <inheritdoc />
        public BindingSource BindingSource => BindingSource.Services;
    }
}
