using Bi.Core.Models;
using System;

namespace Bi.Entities.Input;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/12/14 10:38:03
/// 版本：1.1
/// </summary>

public class BiArticleInput : BaseInput
{
    ///<summary>
    ///id
    ///</summary>
    public string[] Ids { set; get; }
    ///<summary>
    ///id
    ///</summary>
    public string Id  { set; get;} 
	///<summary>
	///title
	///</summary>
	public string Title  { set; get;} 
	///<summary>
	///content
	///</summary>
	public string Content  { set; get;} 
}