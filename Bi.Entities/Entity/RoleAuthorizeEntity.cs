using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Entity;

public class RoleAuthorizeEntity : BaseEntity
{
    /// <summary>
    /// 角色id
    /// </summary>
    public string RoleId { get; set; }

    /// <summary>
    /// 菜单/按钮id
    /// </summary>
    public string MenuButtonId { get; set; }

    /// <summary>
    /// 分类1-菜单 2-按钮
    /// </summary>
    public int? Category { get; set; }

    /// <summary>
    /// 来源1-web 2-app
    /// </summary>
    public int? Source { get; set; }
}