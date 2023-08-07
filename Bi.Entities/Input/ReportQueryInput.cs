using Bi.Core.Models;

namespace Bi.Entities.Input;

public class ReportQueryInput : BaseInput
{
    /// <summary>
    /// 用户权限
    /// </summary>
    public string? CodeList { get; set; }
    /// <summary>
    /// 数据源编码
    /// </summary>
    public string? SetCode { get; set; }
    /// <summary>
    /// 报表编码
    /// </summary>
    public string? ReportCode { get; set; }
    /// <summary>
    /// 报表描述
    /// </summary>
    public string? ReportDesc { get; set; }

    /// <summary>
    /// 报表类型
    /// </summary>
    public string? ReportType { get; set; }

    public string? ReportAuthor { get; set; }

    public string? ReportName { get; set; }
}