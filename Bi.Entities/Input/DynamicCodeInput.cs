using Bi.Entities.Response;

namespace Bi.Entities.Input;

public class DynamicCodeInput
{
    /// <summary>
    /// 动态c#代码语句
    /// </summary>
    public string? DynamicCode { get; set; }
    /// <summary>
    /// 是否为语法检查
    /// </summary>
    public bool CheckFlag { get; set; } = false;
    /// <summary>
    /// 要计算的单元格列表
    /// </summary>
    public List<CellItem>? List { get; set; }

}