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
    public string? Id { set; get; }
    /// <summary>
    /// 批量删除
    /// </summary>
    public string[] multiId { set; get; }
    /// <summary>
    /// 父级菜单id
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// 分类1-菜单 2-按钮
    /// </summary>
    [Range(0, 2)]
    public int Category { get; set; }

    /// <summary>
    /// 来源1-web 2-pda 3-oee
    /// </summary>
    [Range(1, 3)]
    public int Source { get; set; } = 1;

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

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
    /// API 逗号分隔
    /// </summary>
    public string Apis { get; set; }
}

