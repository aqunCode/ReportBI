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

    public string[] multiId { get; set; }
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

}

public class DataItemInput : BaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public string Id { get; set; }
    public string[] multiId { get; set; }
    public string ItemCode { get; set; }

    public string ItemName { get; set; }

    public string ParentId { get; set; }
}
