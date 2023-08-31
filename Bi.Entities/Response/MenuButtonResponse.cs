using Bi.Core.Models;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Response;

/// <summary>
/// 获取菜单、按钮分页列表实体
/// </summary>
[MessagePackObject(true)]
public class MenuButtonResponse : BaseEntity
{
    /// <summary>
    /// 父ID
    /// </summary>
    public string ParentId { get; set; }

    /// <summary>
    ///  分类1-菜单 2-按钮
    /// </summary>
    public int Category { get; set; }

    /// <summary>
    /// 来源1-web 2-pda 3-oee
    /// </summary>
    public int Source { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 跳转
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// 前端组件
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
    /// API
    /// </summary>
    public string Apis { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public List<MenuButtonResponse> Children { get; set; }
}

/// <summary>
/// 获取所有菜单的树状结构
/// </summary>
public class MenuButtonTree
{
    public string? Id { get; set; }

    public string? ParentId { get; set; }

    public string? Title { get; set; }

    public bool Expand { get; set; } = true;

    public List<MenuButtonTree> Children { get; set; }

}
/// <summary>
/// 当前登录用户的所有菜单
/// </summary>
[MessagePackObject(true)]
public class AuthMenuResponse
{
    /// <summary>
    /// 菜单ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 父ID
    /// </summary>
    public string ParentId { get; set; }

    /// <summary>
    /// 类型1-菜单 2-按钮
    /// </summary>
    public int Category { get; set; }

    /// <summary>
    /// 来源1-web 2-pda 3-oee
    /// </summary>
    public int Source { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 跳转
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// 前端组件
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public int? SortCode { get; set; }

    /// <summary>
    /// API
    /// </summary>
    public string Apis { get; set; }

    /// <summary>
    /// Remark
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public List<AuthMenuResponse> Children { get; set; }
}