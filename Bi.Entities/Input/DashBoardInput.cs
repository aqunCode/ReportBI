using Bi.Core.Models;
using Bi.Entities.Entity;

namespace Bi.Entities.Input;

public class DashBoardInput : BaseInput
{
    /// <summary>
    /// 报表编码
    /// </summary>
    public string? ReportCode { get; set; }
    /// <summary>
    /// 大屏画布中的组件
    /// </summary>
    public List<ReportDashboardWidgetInput>? Widgets { get; set; }
    /// <summary>
    /// 大屏画布属性
    /// </summary>
    public ReportDashboard? Dashboard { get; set; }
}

