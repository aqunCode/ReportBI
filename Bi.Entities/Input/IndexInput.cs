using Bi.Core.Attributes;
using Bi.Core.Models;

namespace Bi.Entities.Input;

public class IndexInput
{
    /// <summary>
    /// 工作簿ID
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// 选择的时间类别(月周天)(对应传值 all month week day)
    /// </summary>
    public string? DateType { get ; set; }
    /// <summary>
    /// 选择报表类型 BI 或者 报表设计
    /// </summary>
    public string? ReportType { get; set; }
    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [SwaggerIgnore]
    public CurrentUser? CurrentUser { get; set; }
}

