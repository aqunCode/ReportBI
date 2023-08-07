using Bi.Core.Models;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Response;

/// <summary>
/// 数据字典明细表响应实体
/// </summary>
public class DataItemDetailResponse : BaseEntity
{
    /// <summary>
    /// 数据字典主表Id
    /// </summary>
    public string ItemId { get; set; }

    /// <summary>
    /// 明细编码
    /// </summary>
    public string DetailCode { get; set; }

    /// <summary>
    /// 明细名称
    /// </summary>
    public string DetailName { get; set; }

    /// <summary>
    /// 排序码
    /// </summary>
    public int? SortCode { get; set; }
}
