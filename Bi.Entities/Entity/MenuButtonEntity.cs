using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Entity;

public class MenuButtonEntity : BaseEntity
{
    /// <summary>
    /// 分类1-菜单 2-按钮
    /// </summary>
    public int? Category { get; set; }

    /// <summary>
    /// 来源1-web 2-pda 3-oee
    /// </summary>
    public int? Source { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 父级菜单id
    /// </summary>
    public string ParentId { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// 前台组件
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 排序码
    /// </summary>
    public int? SortCode { get; set; }

    /// <summary>
    /// APIS,该菜单或者按钮IDs 逗号分隔。
    /// </summary>
    public string Apis { get; set; }
}
