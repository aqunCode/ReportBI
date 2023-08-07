using Bi.Core.Models;

namespace Bi.Entities.Input;

public class ReportExcelInput : BaseInput
{
    /// <summary>
    /// 是否是第一次预览
    /// </summary>
    public bool? FirstView { get; set; } = false;
    /// <summary>
    /// 报表名称
    /// </summary>
    public  string? ReportName {get;set;}
    /// <summary>
    /// 报表编码
    /// </summary>
    public string? ReportCode {get;set;}
    /// <summary>
    /// 数据集编码，以|分割
    /// </summary>
    public string? SetCodes {get;set;}
    /// <summary>
    /// 分组
    /// </summary>
    public string? ReportGroup {get;set;}

    /// <summary>
    /// 数据集查询参数
    /// </summary>
    public string? SetParam {get;set;}
    /// <summary>
    /// 报表json字符串
    /// </summary>
    public string? JsonStr {get;set;}
    /// <summary>
    /// 报表类型
    /// </summary>
    public string? ReportType {get;set;}
    /// <summary>
    /// 数据总计
    /// </summary>
    public long? Total {get;set;}
    /// <summary>
    /// 导出类型
    /// </summary>
    public string? ExportType {get;set;}

    public int RequestCount { get; set; } = 0;

    public int PageSize { get; set; } = 0;

    public string? SheetIndex { get; set; }

    public bool Export { get; set; } = false;
}
