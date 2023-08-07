using Newtonsoft.Json.Linq;

namespace Bi.Entities.Input;

public class ReportDashboardWidgetValue
{
    /// <summary>
    /// 报表编码
    /// </summary>
    public string? ReportCode { get; set; }
    /// <summary>
    /// 组件的渲染属性json
    /// </summary>
    public JToken? Setup { get; set; }
    /// <summary>
    /// 组件的数据属性json
    /// </summary>
    public JToken? Data { get; set; }
    /// <summary>
    /// 组件的配置属性json
    /// </summary>
    public JToken? Collapse { get; set; }
    /// <summary>
    /// 组件的大小位置属性json
    /// </summary>
    public JToken? Position { get; set; }
    /// <summary>
    /// options
    /// </summary>
    public string? Options { get; set; }
    /// <summary>
    /// 自动刷新间隔秒
    /// </summary>
    public int? RefreshSeconds { get; set; }
    /// <summary>
    /// 0--已禁用 1--已启用  DIC_NAME=ENABLE_FLAG
    /// </summary>
    public int? EnableFlag { get; set; }
    /// <summary>
    /// 0--已禁用 1--已启用  DIC_NAME=ENABLE_FLAG
    /// </summary>
    public int? DeleteFlag { get; set; }
    /// <summary>
    /// 排序，图层的概念
    /// </summary>
    public long? Sort { get; set; }
}