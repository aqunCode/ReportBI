using Microsoft.Extensions.DependencyInjection;

namespace Bi.Core.Models
{
    /// <summary>
    /// SqlBuilder配置
    /// </summary>
    public class SqlBuilderOptions
    {
        /// <summary>
        /// 数据库默认名称
        /// </summary>
        public string DefaultName { get; set; } = "Base";

        /// <summary>
        /// 是否启用格式化
        /// </summary>
        public bool EnableFormat { get; set; } = false;

        /// <summary>
        /// 分页计数语法，默认：COUNT(*)
        /// </summary>
        public string CountSyntax { get; set; } = "COUNT(*)";

        /// <summary>
        /// 数据库连接是否自动释放，默认：true
        /// </summary>
        public bool AutoDispose { get; set; } = true;

        /// <summary>
        /// 连接字符串配置Section，默认：ConnectionStrings
        /// </summary>
        public string ConnectionSection { get; set; } = "ConnectionStrings";

        /// <summary>
        /// 仓储生命周期
        /// </summary>
        public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Transient;
    }
}
