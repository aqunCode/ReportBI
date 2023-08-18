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
public class MenuButtonAddInput : BaseInput
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

/// <summary>
/// 修改菜单、按钮输入参数
/// </summary>
[MessagePackObject(true)]
public class MenuButtonModifyInput : BaseInput
{
    /// <summary>
    /// 菜单、按钮主键id
    /// </summary>
    [Required]
    public string Id { get; set; }

    /// <summary>
    /// 父级菜单id
    /// </summary>
    [Required]
    public string ParentId { get; set; }

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

/// <summary>
/// 查询菜单、按钮输入参数
/// </summary>
[MessagePackObject(true)]
public class MenuButtonQueryInput : BaseInput
{
    /// <summary>
    /// 菜单、按钮主键id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 父级菜单id
    /// </summary>
    public string ParentId { get; set; }

    /// <summary>
    /// 分类1-菜单 2-按钮
    /// </summary>
    public int? Category { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 来源1-web 2-pda 3-oee
    /// </summary>
    [Required]
    [Range(1, 3)]
    public int Source { get; set; }
}

/// <summary>
/// 查询指定账号授权菜单、按钮输入参数
/// </summary>
[MessagePackObject(true)]
public class AuthMenuButtonQueryInput : BaseInput
{
    /// <summary>
    /// 角色id，多个角色逗号拼接
    /// </summary>
    [Required]
    public string RoleIds { get; set; }

    /// <summary>
    /// 系统标识
    /// </summary>
    [Required]
    public string SystemFlag { get; set; }

    /// <summary>
    /// 分类 0-全部 1-菜单 2-按钮
    /// </summary>
    [Required]
    [Range(0, 2)]
    public int Category { get; set; }

    /// <summary>
    /// 分类 0 获取自己拥有的, 1 获取选择的角色
    /// </summary>
    [Required]
    [Range(0, 1)]
    public int Type { get; set; }

    /// <summary>
    /// 来源1-web 2-pda 3-oee
    /// </summary>
    [Required]
    [Range(1, 3)]
    public int Source { get; set; }
}