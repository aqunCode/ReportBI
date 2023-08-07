using Bi.Core.Models;

namespace Bi.Entities.Input;

public  class ChartInput : BaseInput
{
    public string? ChartType { get; set; }
    /// <summary>
    /// 数据集编码
    /// </summary>
    public string? SetCode { get; set; }
    /// <summary>
    /// 传入的自定义参数
    /// </summary>
    public Dictionary<string,string>? ContextData { get; set; }
    /// <summary>
    /// 图表属性
    /// </summary>
    public Dictionary<string, string>? ChartProperties { get; set; }
    /// <summary>
    /// 时间字段
    /// </summary>
    public string? TimeLineFiled { get; set; }
    /// <summary>
    /// 时间颗粒度
    /// </summary>
    public string? Particles { get; set; }
    /// <summary>
    /// 时间格式化
    /// </summary>
    public string? DataTimeFormat { get; set; }
    /// <summary>
    /// 时间展示层
    /// </summary>
    public string? TimeLineFormat { get; set; }

    public int TimeUnit { get; set; }
    /// <summary>
    /// 时间区间
    /// </summary>
    public string? StartTime { get; set; }
    /// <summary>
    /// 时间区间
    /// </summary>
    public string? EndTime { get; set; }
    /// <summary>
    /// 行列二级操作
    /// </summary>
    public AutoTurn? AutoTurn { get; set; }
}