using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("auto_report_dashboard")]
[MessagePackObject(true)]
public class ReportDashboard : BaseEntity
{
    public string? ReportCode { get; set; }

    public string? Title { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public string? BackgroundColor { get; set; }

    public string? BackgroundImage { get; set; }

    public string? PresetLine { get; set; }

    public int? RefreshSeconds { get; set; }

    public int? DeleteFlag { get; set; }

    public int? Sort { get; set; }

}

