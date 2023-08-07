using Bi.Core.Attributes;
using Bi.Core.Models;

namespace Bi.Entities.Entity;

public class BiRecord
{
    /// <summary>
    /// 模型总数
    /// </summary>
    public int ModelCount { get; set; } = 0;
    /// <summary>
    /// 工作簿总数
    /// </summary>
    public int WorkbookCount { get; set; } = 0;
    /// <summary>
    /// 点击总数
    /// </summary>
    public Double ClickCount { get; set; } = 0;
    /// <summary>
    /// 新增点击
    /// </summary>
    public int AddCount { get; set; } = 0;

}

public class BiFrequency
{
    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { set; get; }
    /// <summary>
    /// 工作簿id
    /// </summary>
    public string? WorkBooId { set; get; }
    /// <summary>
    /// 工作簿名称
    /// </summary>
    public string? WorkBookName { set; get; }
    /// <summary>
    /// 访问次数
    /// </summary>
    public Double ClickCount { set; get; }
    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [SwaggerIgnore]
    public CurrentUser? CurrentUser { get; set; }
}

public class BiChartRecord
{
    /// <summary>
    /// 日期
    /// </summary>
    public string? DateStr { get; set; }
    /// <summary>
    /// 点击总数
    /// </summary>
    public Double ClickCount { get; set; } = 0;

}
public class BiModelRecord
{
    /// <summary>
    /// 日期
    /// </summary>
    public string? ModelName { get; set; }
    /// <summary>
    /// 总数
    /// </summary>
    public Double Counts { get; set; } = 0;

}