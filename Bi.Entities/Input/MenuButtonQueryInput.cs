using Bi.Core.Models;
using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Input;

/// <summary>
/// 新增菜单、按钮输入参数
/// </summary>
[MessagePackObject(true)]
public class MenuButtonInput : BaseInput
{
    /// <summary>
    /// 父级菜单id
    /// </summary>
    [Required]
    public string ParentId { get; set; } = "0";

    /// <summary>
    /// 分类1-菜单 2-按钮
    /// </summary>
    [Required]
    [Range(1, 2)]
    public int Category { get; set; }

    /// <summary>
    /// 来源1-web 2-pda 3-oee
    /// </summary>
    [Required]
    [Range(1, 3)]
    public int Source { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    public string Title { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    [Required]
    public string Href { get; set; }

    /// <summary>
    /// 前台组件
    /// </summary>
    [Required]
    public string Component { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [Required]
    public string Icon { get; set; }

    /// <summary>
    /// API 逗号分隔
    /// </summary>
    public string Apis { get; set; }
}

