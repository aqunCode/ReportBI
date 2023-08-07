using Bi.Core.Models;
using MessagePack;
using System.ComponentModel.DataAnnotations;

namespace Bi.Entities.Input;

[MessagePackObject(true)]
public class ReportInput : BaseInput {
    /// 报表编码
    [Required]
    public string? ReportCode { get; set; }
    /// <summary>
    /// 报表描述
    /// </summary>
    public string? ReportDesc { get; set; }
    /// <summary>
    /// 报表类型
    /// </summary>
    [Required]
    public string? ReportType { get; set; }

    public string? ReportAuthor { get; set; }

    [Required]
    public string? ReportName { get; set; }

}