using Newtonsoft.Json.Linq;

namespace Bi.Entities.Input;

public class ReportDashboardWidgetInput
{
    public string? Type { get; set; }

    public ReportDashboardWidgetValue? Value  { get;set;}

    public JToken? Options  { get; set; }
}