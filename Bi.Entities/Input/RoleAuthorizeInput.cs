using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Input;

public class RoleAuthorizeInput : BaseInput
{
    /// <summary>
    /// guid 主键
    /// </summary>
    public string Id { set; get; }
    /// <summary>
    /// 角色id
    /// </summary>
    public string? RoleId { get; set; }
    /// <summary>
    /// 角色id
    /// </summary>
    public string? RoleName { get; set; }
    /// <summary>
    /// 菜单/按钮id
    /// </summary>
    public string? MenuButtonId { get; set; }
}
