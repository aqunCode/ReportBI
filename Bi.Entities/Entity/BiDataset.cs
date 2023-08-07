using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity;
/// <summary>
/// 数据集
/// </summary>
[SugarTable("BI_DATASET")]
public class BiDataset:BaseEntity
{
    ///<summary>
    ///数据源编码
    ///</summary>
    public string? SourceCode { set; get; }
    ///<summary>
    ///数据集名称
    ///</summary>
    public string? DatasetName { set; get; }
    ///<summary>
    ///数据集编码
    ///</summary>
    public string? DatasetCode { set; get; }
    ///<summary>
    ///数据集sql
    ///</summary>
    public string? Content { set; get; }
    ///<summary>
    ///是否删除
    ///</summary>
    public string? DeleteFlag { set; get; } = "N";
    ///<summary>
    ///备用1
    ///</summary>
    public string? Opt1 { set; get; }
    ///<summary>
    ///备用2
    ///</summary>
    public string? Opt2 { set; get; }
    ///<summary>
    ///备用3
    ///</summary>
    public string? Opt3 { set; get; }
    ///<summary>
    ///备用4
    ///</summary>
    public string? Opt4 { set; get; }
    ///<summary>
    ///备用5
    ///</summary>
    public string? Opt5 { set; get; }
}

