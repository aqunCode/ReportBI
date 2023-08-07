using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Bi.Core.Helpers;

namespace Bi.Core.Const
{
    /// <summary>
    /// appsettings配置信息
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static AppSettings()
        {
            Configuration = ConfigHelper.Configuration;
        }

        /// <summary>
        /// appsettings配置
        /// </summary>
        public static IConfiguration Configuration { get; private set; }

        /// <summary>
        /// 超级管理员账号
        /// </summary>
        public static List<string> Administrators =>
            Configuration.GetSection("Administrators").Get<List<string>>();

        /// <summary>
        /// 判断是否管理员账号
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static bool IsAdministrator(string account) =>
            Administrators.Any(x => x == account);
    }
}
