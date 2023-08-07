namespace Bi.Entities.Response;

public class ReportExcelOutput
{
    /// <summary>
    /// 报表名称
    /// </summary>
    public string? ReportName { get; set; }
    /// <summary>
    /// 报表编码
    /// </summary>
    public string? ReportCode { get; set; }
    /// <summary>
    /// 数据集编码，以|分割
    /// </summary>
    public string? SetCodes { get; set; }
    /// <summary>
    /// 数据集名称，以|分割
    /// </summary>
    public string? SetNames { get; set; }
    /// <summary>
    /// 分组
    /// </summary>
    public string? ReportGroup { get; set; }
    /// <summary>
    /// 数据集查询参数
    /// </summary>
    public string? SetParam { get; set; }
    /// <summary>
    /// 报表json字符串
    /// </summary>
    public string? JsonStr { get; set; }
    /// <summary>
    /// 报表类型
    /// </summary>
    public string? ReportType { get; set; }
    /// <summary>
    /// 数据总计
    /// </summary>
    public long? Total { get; set; }
    /// <summary>
    /// 导出类型
    /// </summary>
    public string? ExportType { get; set; }

    public int RequestCount { get; set; } = 0;

    public int PageSize { get; set; } = 0;

    public int PageCount { get; set; } = 0;

    public double? TimeSpan { get; set; }

    public string? SheetIndex { get; set; }

    public bool Export { get; set; } = false;
}
