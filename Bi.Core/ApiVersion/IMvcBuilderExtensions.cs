using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Bi.Core.ApiVersion;
/// <summary>
/// IMvcBuilder扩展类
/// </summary>
public static class IMvcBuilderExtensions
{
    /// <summary>
    /// ApiVersionNeutral扩展方法
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static IMvcBuilder AddApiVersionNeutral(this IMvcBuilder @this)
    {
        @this.Services.Configure<MvcOptions>(options =>
            options.Conventions.Add(new ApiControllerVersionConvention()));

        return @this;
    }
}