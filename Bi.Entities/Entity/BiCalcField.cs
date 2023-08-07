using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity;
/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/1/9 15:38:50
/// 版本：1.1
/// </summary>
[SugarTable("BI_CALC_FIELD")]
public class BiCalcField:BaseEntity
{

	///<summary>
	///WORKBOOKID
	///</summary>
	public string? WorkbookId  { set; get;} 
	///<summary>
	///NODEID
	///</summary>
	public string? NodeId  { set; get;} 
	///<summary>
	///LABELNAME
	///</summary>
	public string? LabelName  { set; get;} 
	///<summary>
	///COLUMNNAME
	///</summary>
	public string? ColumnName  { set; get;}
	///<summary>
	///ColumnRename列重命名
	///</summary>
	public string? ColumnRename { set; get; }
	///<summary>
	///行列字段的计算函数
	///</summary>
	public string? CalculatorFunction { set; get; }
    ///<summary>
    ///COLUMNTYPE
    ///</summary>
    public string? ColumnType  { set; get; }
    ///<summary>
    ///数据类型
    ///</summary>
    public string? DataType { set; get; }
	/// <summary>
	/// 是否为连续型？  0-离散型(维度) 1-连续型(指标)
	/// </summary>
	public int? IsContinue { get; set; }
	///<summary>
	///AXIS
	///</summary>
	public string? Axis  { set; get;} 
	///<summary>
	///ORDERBY
	///</summary>
	public decimal OrderBy  { set; get; }
    ///<summary>
    ///自定义排序字段
    ///</summary>
    public string? SortBy { set; get; }
    ///<summary>
    ///自定义排序类型
    ///</summary>
    public string? SortType { set; get; }
    ///<summary>
    ///手动排序的话排序列表
    ///</summary>
	[SugarColumn(IsIgnore = true)]
    public List<SortField>? SortValue { set; get; }
    ///<summary>
    ///手动排序的话排序列表(存储的json串)
    ///</summary>
    public string? SortValueJson { set; get; }
    ///<summary>
    ///OPT1
    ///</summary>
    public string? Opt1  { set; get;} 
	///<summary>
	///OPT2
	///</summary>
	public string? Opt2  { set; get;} 
	///<summary>
	///OPT3
	///</summary>
	public string? Opt3  { set; get;} 
	///<summary>
	///OPT4
	///</summary>
	public string? Opt4  { set; get;} 
	///<summary>
	///DELETEFLAG
	///</summary>
	public string? DeleteFlag  { set; get;} 
}

