using MessagePack;
using System;

namespace Bi.Core.Models
{
    /// <summary>
    /// 当前用户信息
    /// </summary>
    [MessagePackObject(true)]
    public class CurrentUser : BaseEntity
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string? Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }

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
        /// 排序码
        /// </summary>
        public int SortCode { get; set; }

        /// <summary>
        /// 角色ids
        /// </summary>
        public string? RoleIds { get; set; }

        /// <summary>
        /// 公司ids
        /// </summary>
        public string CompanyIds { get; set; }

        /// <summary>
        /// 部门ids
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
        public bool IsAdministrator { get; set; }

        /// <summary>
        /// 上次修改密码时间
        /// </summary>
        public DateTime? LastPasswordChangeTime { get; set; }
    }
}
