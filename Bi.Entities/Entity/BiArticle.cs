using Bi.Core.Models;
using SqlSugar;
using System;

namespace Bi.Entities.Entity;
/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/12/14 10:38:03
/// 版本：1.1
/// </summary>
[SugarTable("bi_article")]
public class BiArticle:BaseEntity
{

	///<summary>
	///title
	///</summary>
	public string Title  { set; get;} 
	///<summary>
	///content
	///</summary>
	public string Content  { set; get;} 
}

