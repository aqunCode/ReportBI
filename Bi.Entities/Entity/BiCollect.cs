using Bi.Core.Models;
using SqlSugar;
using System;

namespace Bi.Entities.Entity;
/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：7/4/2023 3:58:57 PM
/// 版本：1.1
/// </summary>
[SugarTable("bi_collect")]
public class BiCollect:BaseEntity
{

	///<summary>
	///USERID
	///</summary>
	public string? UserId  { set; get;} 
	///<summary>
	///COLLECT
	///</summary>
	public string? Collect  { set; get;} 
	///<summary>
	///TYPE
	///</summary>
	public string? Type  { set; get;}
    ///<summary>
    ///Name
    ///</summary>
    public string? Name  { set; get;} 
	///<summary>
	///OPT2
	///</summary>
	public string? Opt1 { set; get;} 
}

