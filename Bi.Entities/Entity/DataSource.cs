using Bi.Core.Helpers;
using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("auto_data_source")]
[MessagePackObject(true)]
public class DataSource  : BaseEntity{
    /// <summary>
    /// 数据源编码
    /// </summary>
    [ExcelColumn("数据源编码")]
    public string? SourceCode { get; set; }
    /// <summary>
    /// 数据源名称
    /// </summary>
    [ExcelColumn("数据源名称")]
    public string? SourceName { get; set; }
    /// <summary>
    /// 数据源描述
    /// </summary>
    [ExcelColumn("数据源描述")]
    public string? SourceDesc { get; set; }
    /// <summary>
    /// 数据源类型
    /// </summary>
    [ExcelColumn("数据源类型")]
    public string? SourceType { get; set; }
    /// <summary>
    /// DB连接串
    /// </summary>
    [ExcelColumn("DB连接串")]
    public string? SourceConnect { get; set; }
    /// <summary>
    /// 是否删除
    /// </summary>
    [ExcelColumn("是否删除")]
    public int DeleteFlag { get; set; }
    /// <summary>
    /// http 请求路径
    /// </summary>
    [ExcelColumn("请求路径")]
    public string? HttpAddress { get; set; }
    /// <summary>
    /// http 请求方式
    /// </summary>
    [ExcelColumn("请求方式")]
    public string? HttpWay { get; set; }
    /// <summary>
    /// http 请求头
    /// </summary>
    [ExcelColumn("请求头")]
    public string? HttpHeader { get; set; }
    /// <summary>
    /// http 请求体
    /// </summary>
    [ExcelColumn("请求体")]
    public string? HttpBody { get; set; }
}

