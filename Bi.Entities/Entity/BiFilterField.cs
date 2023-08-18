using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity;
/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/1/9 15:37:11
/// 版本：1.1
/// </summary>
[SugarTable("bi_filter_field")]
public class BiFilterField:BaseEntity
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
	///COLUMNTYPE
	///</summary>
	public string? ColumnType  { set; get;}
	///<summary>
	///FILTERVALUE
	///</summary>
	public string? FilterValue  { set; get;}
	/// <summary>
	/// 是否在预览界面展示 0-不展示 1-展示 默认1
	/// </summary>
	public int IsShow { set; get; }
    ///<summary>
    ///ORDERBY
    ///</summary>
    public int OrderBy { set; get; }
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
	///DELETEFLAG
	///</summary>
	public int? DeleteFlag  { set; get;} 
}

