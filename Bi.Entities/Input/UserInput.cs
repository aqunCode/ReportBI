using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Input;

public class UserInput : BaseInput
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// 批量删除
    /// </summary>
    public string[] MultiId { get; set; }
    /// <summary>
    /// 姓名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string? HeadIcon { get; set; }

    /// <summary>
    /// 简拼
    /// </summary>
    public string? SimpleSpelling { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 角色ids
    /// </summary>
    public string? RoleIds { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    public string CompanyIds { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    public string DepartmentIds { get; set; }

    /// <summary>
    /// 系统标识
    /// </summary>
    public string? SystemFlag { get; set; }

    /// <summary>
    /// 系统来源：1-web 2-app
    /// </summary>
    public int Source { get; set; }

    /// <summary>
    /// 管理员
    /// </summary>
    public int IsAdministrator { get; set; }

}
