using Bi.Entities.Response;

namespace Bi.Entities.Response;

public class DashBoardOutput
{
    /// <summary>
    /// 报表编码
    /// </summary>
    public string? ReportCode { get; set; }
    /// <summary>
    /// 大屏画布中的组件
    /// </summary>
    public List<ReportDashboardWidgetOutput>? Widgets { get; set; }
    /// <summary>
    /// 大屏画布属性
    /// </summary>
    public ReportDashboardOutput? Dashboard { get; set; }
}