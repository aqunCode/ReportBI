using SqlSugar;

namespace Bi.Core.SqlSugar;

public class SqlSugarOptins
{
    /// <summary>
    /// 连接串名称
    /// </summary>
    public string? ConnId { get; set; }
    /// <summary>
    /// 数据库连接池
    /// </summary>
    public string? ConnString { get; set; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public DbType DbType { get; set; }
}

