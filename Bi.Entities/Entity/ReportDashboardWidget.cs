using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("bi_report_dashboard_widget")]
[MessagePackObject(true)]
public class ReportDashboardWidget : BaseEntity
{
    public string? ReportCode { get; set; }

    public string? Type { get; set; }

    public string? Setup { get; set; }

    public string? Data { get; set; }

    public string? Collapse { get; set; }

    public string? Position { get; set; }

    public string? Options { get; set; }

    public int RefreshSeconds { get; set; }

    private int EnableFlag { get; set; }

    public int DeleteFlag { get; set; }

    public int Sort { get; set; }
}
