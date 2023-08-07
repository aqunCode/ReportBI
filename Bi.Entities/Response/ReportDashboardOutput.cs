namespace Bi.Entities.Response;

public  class ReportDashboardOutput
{
    public string? ReportCode { get; set; }

    public string? Title { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public string? BackgroundColor { get; set; }

    public string? BackgroundImage { get; set; }

    public string? PresetLine { get; set; }

    public int? RefreshSeconds { get; set; }


    public int? DeleteFlag { get; set; }

    public int? Sort { get; set; }

    public List<ReportDashboardWidgetOutput>? Widgets { get; set; }
}