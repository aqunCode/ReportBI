using Bi.Core.Helpers;
using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("auto_chart_item")]
[MessagePackObject(true)]
public class DataDict : BaseEntity {
    /*/// <summary>
    /// 查询条件列表
    /// </summary>
    public List<DataCollectItem> dataSetParamDtoList {set;get;}*/
    /// <summary>
    /// 返回结果集
    /// </summary>
    [ExcelColumn("图表类型")]
    public string? ChartType { set; get; }
    /// <summary>
    /// 数据集类型
    /// </summary>
    [ExcelColumn("数据类型名称")]
    public string? DataName { set; get; }
    /// <summary>
    /// 数据源编码
    /// </summary>
    [ExcelColumn("数据类型")]
    public string? DataType { set; get; }
    /// <summary>
    /// 是否删除
    /// </summary>
    [ExcelColumn("是否删除")]
    public int DeleteFlag { get; set; }
}

