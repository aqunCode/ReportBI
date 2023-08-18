using Bi.Core.Helpers;
using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("bi_data_collect_item")]
[MessagePackObject(true)]
public class DataCollectItem  : BaseEntity{
    /// <summary>
    /// 数据集编码
    /// </summary>
    [ExcelColumn("数据集编码")]
    public string? SetCode { get; set; }
    /// <summary>
    /// 排序码
    /// </summary>
    [ExcelColumn("排序码")]
    public int SortCode { get; set; }
    /// <summary>
    /// 参数名称
    /// </summary>
    [ExcelColumn("参数名称")]
    public string? ParamName { get; set; }
    /// <summary>
    /// 参数类型
    /// </summary>
    [ExcelColumn("参数类型")]
    public string? ParamType { get; set; }
    /// <summary>
    /// 参数限制
    /// </summary>
    [ExcelColumn("参数限制")]
    public string? ParamAstrict { get; set; }
    /// <summary>
    /// 参数描述
    /// </summary>
    [ExcelColumn("参数描述")]
    public string? ParamDesc { get; set; }
    /// <summary>
    /// 是否必填
    /// </summary>
    [ExcelColumn("是否必填")]
    public int RequiredFlag { get; set; }
    /// <summary>
    /// 示例参数
    /// </summary>
    [ExcelColumn("示例参数")]
    public string? SampleItem { get; set; }
    /// <summary>
    /// 效验规则
    /// </summary>
    [ExcelColumn("效验规则")]
    public string? ValidationRules { get; set; }
    /// <summary>
    /// 是否删除
    /// </summary>
    [ExcelColumn("是否删除")]
    public int DeleteFlag { get; set; }
}

