using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("bi_report")]
[MessagePackObject(true)]
public class AutoReport : BaseEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    public String? ReportName { get; set; }


    /// <summary>
    /// 报表编码
    /// </summary>
    public String? ReportCode { get; set; }

    /// <summary>
    /// 分组
    /// </summary>
    public String? ReportGroup { get; set; }

    /// <summary>
    /// 报表描述
    /// </summary>
    public String? ReportDesc { get; set; }

    /// <summary>
    /// 报表类型
    /// </summary>
    public String? ReportType { get; set; }

    /// <summary>
    /// 报表缩略图
    /// </summary>
    public String? ReportImage { get; set; }

    /// <summary>
    /// 报表作者
    /// </summary>
    public String? ReportAuthor { get; set; }
    /// <summary>
    /// 下载次数
    /// </summary>
    public int DownloadCount { get; set; }

    /// <summary>
    /// 0--未删除 1--已删除 DIC_NAME=DELETE_FLAG
    /// </summary>
    public int DeleteFlag { get; set; }
}

