using Bi.Core.Models;

namespace Bi.Entities.Input;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：7/3/2023 11:21:23 AM
/// 版本：1.1
/// </summary>
public class BiCollectInput : BaseInput
{

	///<summary>
	///ID
	///</summary>
	public string? Id  { set; get;} 
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
	public string? Opt2  { set; get;} 
}