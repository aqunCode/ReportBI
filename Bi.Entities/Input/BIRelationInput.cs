using Bi.Core.Models;

namespace Bi.Entities.Input;

public class BIRelationInput:BaseInput
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public string? Id { set; get; }
    ///<summary>
    /// 工作簿ID
    ///</summary>
    public string? DatasetCode { set; get; }
    ///<summary>
    /// 数据集id
    ///</summary>
    public string? DatasetId { set; get; }
    ///<summary>
    /// 父数据集id
    ///</summary>
    public string? FatherId { set; get; }
    ///<summary>
    /// 层级
    ///</summary>
    public string? TopLevel { set; get; }
    ///<summary>
    /// 连接
    ///</summary>
    public string? JoinRelational { set; get; }
    ///<summary>
    /// 备用1
    ///</summary>
    public string? Opt1 { set; get; }
    ///<summary>
    /// 备用2
    ///</summary>
    public string? Opt2 { set; get; }
    ///<summary>
    /// 备用3
    ///</summary>
    public string? Opt3 { set; get; }
    ///<summary>
    /// 备用4
    ///</summary>
    public string? Opt4 { set; get; }
    ///<summary>
    /// 备用5
    ///</summary>
    public string? Opt5 { set; get; }
}
