﻿using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable(tableName: "bi_dataset_relational")]
public class BIRelation : BaseEntity
{
    ///<summary>
    /// 工作簿ID
    ///</summary>
    public string? DatasetCode { set; get; }
    ///<summary>
    /// 数据集id
    ///</summary>
    public string? SourceId { set; get; }
    ///<summary>
    /// 父数据集id
    ///</summary>
    public string? TargetId { set; get; }
    ///<summary>
    /// 层级
    ///</summary>
    public string? TopLevel { set; get; }
    ///<summary>
    /// 连接
    ///</summary>
    public string? JoinRelational { set; get; }
    ///<summary>
    /// 连接方式
    ///</summary>
    public string? IncidenceRelation { set; get; }
    ///<summary>
    /// 是否删除
    ///</summary>
    public int DeleteFlag { set; get; } = 0;
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
}
