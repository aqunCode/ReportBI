using FluentFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bi.Core.Ftp
{
    /// <summary>
    /// FTP扩展类
    /// </summary>
    public static class FtpExtensions
    {
        /// <summary>
        /// 注入FtpClient
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration"></param>
        /// <param name="lifeTime"></param>
        /// <returns></returns>
        public static IServiceCollection AddFtpClient(
            this IServiceCollection @this,
            IConfiguration configuration,
            ServiceLifetime lifeTime = ServiceLifetime.Transient)
        {
            var host = configuration.GetValue<string>("FtpClient:Host");
            var user = configuration.GetValue<string>("FtpClient:User");
            var pass = configuration.GetValue<string>("FtpClient:Password");

            switch (lifeTime)
            {
                case ServiceLifetime.Singleton:
                    @this.AddSingleton<IFtpClient>(x => new FtpClient(host, user, pass));
                    break;
                case ServiceLifetime.Scoped:
                    @this.AddScoped<IFtpClient>(x => new FtpClient(host, user, pass));
                    break;
                case ServiceLifetime.Transient:
                    @this.AddTransient<IFtpClient>(x => new FtpClient(host, user, pass));
                    break;
                default:
                    break;
            }

            return @this;
        }
    }
}
