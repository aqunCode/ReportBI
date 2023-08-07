using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity;
[SugarTable("BI_MARK_FIELD")]
public class BiMarkField : BaseEntity
{

	///<summary>     
	///WORKBOOKID
	///</summary> 
	public string? WorkbookId { set; get; }
    ///<summary>     
    ///数据集ID
    ///</summary> 
    [SugarColumn(IsIgnore = true)]
    public string? DatasetId { set; get; }
    ///<summary>
    ///NODEID
    ///</summary>
    public string? NodeId { set; get; }
	///<summary>
	///LABELNAME
	///</summary>
	public string? LabelName { set; get; }
	///<summary>
	///COLUMNNAME
	///</summary>
	public string? ColumnName { set; get; }
	///<summary>
	///ColumnRename列重命名
	///</summary>
	public string? ColumnRename { set; get; }
	///<summary>
	///COLUMNTYPE
	///</summary>
	public string? ColumnType { set; get; }
    ///<summary>
    ///分析之后的数据类型
    ///</summary>
    public string? DataType { set; get; }
    ///<summary>
    ///MarkPart 标记所属
    ///</summary>
    public string? MarkPart { set; get; }
	/// <summary>
	/// MarkType
	/// </summary>
	public string? MarkType { set; get; }
	/*/// <summary>
	/// MarkValue  不再接收此参数，因为标记颜色和标记其他参数不一致
	/// </summary>
	public List<MarkValue> MarkValue { set; get; }*/
	/// <summary>
	/// CalculatorFunction
	/// </summary>
	public string? CalculatorFunction { set; get; }
	/// <summary>
	/// Axis
	/// </summary>
	public string? Axis { set; get; }
	///<summary>
	///OrderBy
	///</summary>
	public string? OrderBy { set; get; }
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
    ///手动排序的话排序列表
    ///</summary>
    public string? SortValueJson { set; get; }
    ///<summary>
    ///DELETEFLAG
    ///</summary>
    public string? DeleteFlag { set; get; }
	///<summary>
	///OPT1
	///</summary>
	public string? Opt1 { set; get; }
	///<summary>
	///OPT2
	///</summary>
	public string? Opt2 { set; get; }
	///<summary>
	///OPT3
	///</summary>
	public string? Opt3 { set; get; }
	///<summary>
	///OPT4
	///</summary>
	public string? Opt4 { set; get; }
		
}

