namespace Bi.Entities.Input;

public class AutoTurn
{
    /// <summary>
    /// 选择哪些字段
    /// </summary>
    public List<Column>? Rows { get; set; }
    /// <summary>
    /// 选择哪些数据集
    /// </summary>
    public List<Column>? Columns { get; set; }
}
public class Column
{
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 属性 正常字段 normal 或者 "" 自定义语法 calculator
    /// </summary>
    public string? CalcType { get; set; }
    /// <summary>
    /// 执行function
    /// </summary>
    public string? Function { get; set; }
    /// <summary>
    /// 筛选值
    /// </summary>
    public string[]? Values { get; set; }
}