using Bi.Core.Helpers;
using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("auto_turn")]
[MessagePackObject(true)]
public class AutoTurns : BaseEntity
{

    /// <summary>
    /// 数据集编码
    /// </summary>
    [ExcelColumn("数据集编码")]
    public string? SetCode { set; get; }

    /// <summary>
    /// 类型 是行或者列
    /// </summary>
    [ExcelColumn("类型")]
    public string? Turntype { set; get; }
    /// <summary>
    /// 字段名称
    /// </summary>
    [ExcelColumn("字段名称")]
    public string? Name { set; get; }
    /// <summary>
    /// 属性 正常字段 normal 或者 "" 原始值自定义语法 calcBefore 结果值自定义语法 calcAfter
    /// </summary>
    [ExcelColumn("计算类型")]
    public string? CalcType { set; get; }
    /// <summary>
    /// 要执行的操作
    /// </summary>
    [ExcelColumn("要执行的操作")]
    public string? Function { set; get; }
    /// <summary>
    /// 要筛选的值
    /// </summary>
    [ExcelColumn("要筛选的值")]
    public string? Value { set; get; }
}