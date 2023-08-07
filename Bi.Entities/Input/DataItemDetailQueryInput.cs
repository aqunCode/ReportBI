using Bi.Core.Models;
using MessagePack;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Input;

/// <summary>
/// 数据字典明细检索输入实体
/// </summary>
[MessagePackObject(true)]
public class DataItemDetailQueryInput : BaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 数据字典主表编码
    /// </summary>
    public string ItemCode { get; set; }

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
    /// 排序字段
    /// </summary>
    public string OrderBy { get; set; }

    /// <summary>
    /// 是否升序
    /// </summary>
    public OrderType OrderType { get; set; }

}
