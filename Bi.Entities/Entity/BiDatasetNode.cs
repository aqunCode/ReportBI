using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity;
/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/1/2 15:28:09
/// 版本：1.0
/// </summary>
[SugarTable("BI_DATASET_NODE")]
public class BiDatasetNode:BaseEntity
{

	///<summary>
	///DATASETCODE
	///</summary>
	public string? DatasetCode  { set; get;} 
	///<summary>
	///NODEID
	///</summary>
	public string? NodeId  { set; get;} 
	///<summary>
	///NODELABLE
	///</summary>
	public string? NodeLabel  { set; get;} 
	///<summary>
	///SOURCECODE
	///</summary>
	public string? SourceCode  { set; get;} 
	///<summary>
	///TABLENAME
	///</summary>
	public string? TableName  { set; get;} 
	///<summary>
	///TOPLEVEL
	///</summary>
	public string? TopLevel  { set; get;} 
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
	///OPT5
	///</summary>
	public string? Opt5  { set; get;}
	///<summary>
	///DELETEFLAG
	///</summary>
	public string DeleteFlag { set; get; } = "N";

    ///<summary>
    ///condition Where 条件
    ///</summary>
    public string? Condition { set; get; }

}

