using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("auto_report_dashboard_widget")]
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

    public Int32 RefreshSeconds { get; set; }

    private Int16 EnableFlag { get; set; }

    public Int32 DeleteFlag { get; set; }

    public Int32 Sort { get; set; }
}
