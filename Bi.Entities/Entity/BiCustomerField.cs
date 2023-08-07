using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity;
/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2022/12/28 17:01:12
/// 版本：1.0
/// </summary>
[SugarTable("BI_CUSTOMER_FIELD")]
public class BiCustomerField : BaseEntity
{
    ///<summary>
    ///数据集ID
    ///</summary>
    public string? DatasetId { set; get; }
    ///<summary>
    ///显示表名
    ///</summary>
    public string? LabelName { set; get; }
    ///<summary>
    ///字段名称
    ///</summary>
    public string? FieldCode { set; get; }
    ///<summary>
    ///字段设定
    ///</summary>
    public string? FieldFunction { set; get; }
    ///<summary>
    ///指标维度转换
    ///</summary>
    public int TypeConvert { set; get; } = -1;
    //数据类型（指标/维度）
    public string? DataType { set; get; }
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
    ///<summary>
    ///是否删除
    ///</summary>
    public string DeleteFlag { set; get; } = "N";
}

