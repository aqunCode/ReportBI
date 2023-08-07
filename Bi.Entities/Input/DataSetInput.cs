using Bi.Core.Models;

namespace Bi.Entities.Input;

public class DataSetInput : BaseInput
{
    ///<summary>
    ///数据集编码
    ///</summary>
    public string? Id { set; get; }
    ///<summary>
    ///数据源编码
    ///</summary>
    public string? SourceCode { set; get; }
    ///<summary>
    ///数据集名称
    ///</summary>
    public string? DatasetName { set; get; }
    ///<summary>
    ///数据集编码
    ///</summary>
    public string? DatasetCode { set; get; }
    ///<summary>
    ///数据集sql
    ///</summary>
    public string? Content { set; get; }
    ///<summary>
    ///备用1
    ///</summary>
    public string? Opt1 { set; get; }
    ///<summary>
    ///备用2
    ///</summary>
    public string? Opt2 { set; get; }
    ///<summary>
    ///备用3
    ///</summary>
    public string? Opt3 { set; get; }
    ///<summary>
    ///备用4
    ///</summary>
    public string? Opt4 { set; get; }
    ///<summary>
    ///备用5
    ///</summary>
    public string? Opt5 { set; get; }
}

public class TableInput
{
    /// <summary>
    /// 数据源编码
    /// </summary>
    public string? SourceCode { get; set; }
    /// <summary>
    /// 表名
    /// </summary>
    public string? TableName { get; set; }
    /// <summary>
    /// 用户/database/schema
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// 类型 Table:单表,Sql:自定义SQL
    /// </summary>
    public string Type { get; set; } = "Table";
}