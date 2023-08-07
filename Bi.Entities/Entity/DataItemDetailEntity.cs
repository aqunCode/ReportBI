using Bi.Core.Models;
using MessagePack;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Entity;

/// <summary>
/// 数据字典明细表
/// </summary>
[SugarTable("SYS_DATAITEM_DETAIL")]
public class DataItemDetailEntity : BaseEntity
{
    /// <summary>
    /// 数据字典主表Id
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>
    /// 明细编码
    /// </summary>
    public string? DetailCode { get; set; }

    /// <summary>
    /// 明细名称
    /// </summary>
    public string? DetailName { get; set; }

    /// <summary>
    /// 排序码
    /// </summary>
    public int SortCode { get; set; }
}
