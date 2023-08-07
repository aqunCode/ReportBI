using Bi.Core.Helpers;
using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("auto_data_collect")]
[MessagePackObject(true)]
public class DataCollect : BaseEntity{
    /*/// <summary>
    /// 查询条件列表
    /// </summary>
    public List<DataCollectItem> DataSetParamDtoList {set;get;}*/
    /// <summary>
    /// 返回结果集
    /// </summary>
    [ExcelColumn("返回结果集")]
    public string? CaseResult { set;get;}
    /// <summary>
    /// 数据集类型
    /// </summary>
    [ExcelColumn("数据集类型")]
    public string? SetType { set;get;}
    /// <summary>
    /// 数据源编码
    /// </summary>
    [ExcelColumn("数据源编码")]
    public string? SourceCode { set;get;}
    /// <summary>
    /// 数据源名称
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [ExcelColumn("数据源名称")]
    public string? SourceName { set;get;}
    /*/// <summary>
    /// 数据转换
    /// </summary>
    [ExcelColumn(IsExport = false)]
    public List<string> DataSetTransformDtoList { set;get; }*/
    /// <summary>
    /// 数据集编码
    /// </summary>
    [ExcelColumn("数据集编码")]
    public string? SetCode { set;get; }
    /// <summary>
    /// 数据集名称
    /// </summary>
    [ExcelColumn("数据集名称")]
    public string? SetName { set;get; }
    /// <summary>
    /// 数据集描述
    /// </summary>
    [ExcelColumn("数据集描述")]
    public string? SetDesc { set;get; }
    /// <summary>
    /// 动态sql
    /// </summary>
    [ExcelColumn("动态sql")]
    public string? DynSentence { set;get; }
    /// <summary>
    /// 是否删除
    /// </summary>
    [ExcelColumn("是否删除")]
    public int DeleteFlag { get; set; }
}

