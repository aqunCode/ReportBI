using Bi.Core.Models;

namespace Bi.Entities.Input;

public class DataCollectItemInput : BaseInput {
    /// <summary>
    /// 参数名称
    /// </summary>
    public string? ParamName { get; set; }
    /// <summary>
    /// 参数类型
    /// </summary>
    public string? ParamType { get; set; }
    /// <summary>
    /// 参数描述
    /// </summary>
    public string? ParamDesc { get; set; }
    /// <summary>
    /// 是否必填
    /// </summary>
    public string? RequiredFlag { get; set; }
    /// <summary>
    /// 示例参数
    /// </summary>
    public string? SampleItem { get; set; }
    /// <summary>
    /// 效验规则
    /// </summary>
    public string? ValidationRules { get; set; }
}

