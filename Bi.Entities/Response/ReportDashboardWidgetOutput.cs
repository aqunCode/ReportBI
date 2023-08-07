using Bi.Entities.Input;
using Newtonsoft.Json.Linq;

namespace Bi.Entities.Response;

public class ReportDashboardWidgetOutput
{
    public string? Type { get; set; }

    public ReportDashboardWidgetValue? Value { get; set; }

    public JToken? Options { get; set; }
}
