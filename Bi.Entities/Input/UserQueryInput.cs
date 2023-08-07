using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Input;

/// <summary>
/// 用户查询输入参数
/// </summary>
public class UserQueryInput : BaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    public string RoleId { get; set; }

    /// <summary>
    /// 公司/部门id
    /// </summary>
    public string OrganizeId { get; set; }

    /// <summary>
    /// 是否使用缓存
    /// </summary>
    public bool UseCache { get; set; } = true;
}
