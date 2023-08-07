using Bi.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Bi.Core.EasyFS
{
    /// <summary>
    /// EasyFs扩展类
    /// </summary>
    public static class EasyFSExtensions
    {
        /// <summary>
        /// AddEasyFs
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddEasyFS(this IServiceCollection @this, IConfiguration configuration)
        {
            var children = configuration?.GetChildren();

            if (children?.Any(x => x.Key.EqualIgnoreCase("EasyFS")) == true)
                children = configuration.GetSection("EasyFS")?.GetChildren();

            if (children.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(configuration));

            //判断是否禁用EasyFS
            if (children.FirstOrDefault(x => x.Key.EqualIgnoreCase("Enabled"))?.Value.EqualIgnoreCase("false") == true)
                return @this;

            var appId = children.FirstOrDefault(x => x.Key.EqualIgnoreCase("AppId"))?.Value;
            var appKey = children.FirstOrDefault(x => x.Key.EqualIgnoreCase("AppKey"))?.Value;
            var serverUrl = children.FirstOrDefault(x => x.Key.EqualIgnoreCase("ServerUrl"))?.Value;

            if (appId.IsNotNullOrEmpty() && appKey.IsNotNullOrEmpty() && serverUrl.IsNotNullOrEmpty())
                @this.AddSingleton<IEasyFSService>(p => new EasyFSService(appId, appKey, serverUrl));

            return @this;
        }
    }
}
