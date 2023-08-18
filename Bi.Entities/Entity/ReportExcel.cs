using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("bi_report_excel")]
[MessagePackObject(true)]
public class ReportExcel : BaseEntity
{
    /// <summary>
    /// excel报表编码
    /// </summary>
    public string? ReportCode { get; set; }
    /// <summary>
    /// 数据集列表，逗号分割
    /// </summary>
    public string? SetCodes { get; set; }
    /// <summary>
    /// 数据集参数列表，一个对象里面的不同的key value
    /// </summary>
    public string? SetParam { get; set; }
    /// <summary>
    /// 数据集参数demo值
    /// </summary>
    public string? JsonStr { get; set; }
    /// <summary>
    /// 是否删除
    /// </summary>
    public Int32 DeleteFlag { get; set; }
}

