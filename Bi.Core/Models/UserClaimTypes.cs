using System.IO;

namespace Bi.Core.Models
{
    /// <summary>
    /// 用户自定义Claim类型
    /// </summary>
    public static class UserClaimTypes
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public const string UserId = "user_id";

        /// <summary>
        /// 用户账户
        /// </summary>
        public const string Account = "account";

        /// <summary>
        /// 头像图标
        /// </summary>
        public const string HeadIcon = "head_icon";

        /// <summary>
        /// 系统标识
        /// </summary>
        public const string SystemFlag = "system_flag";

        /// <summary>
        /// 来源：1-web 2-pda 3-oee
        /// </summary>
        public const string Source = "source";

        /// <summary>
        /// 角色Id
        /// </summary>
        public const string RoleId = "role_id";

        /// <summary>
        /// 公司Id
        /// </summary>
        public const string CompanyId = "company_id";

        /// <summary>
        /// 部门Id
        /// </summary>
        public const string DepartmentId = "department_id";

        /// <summary>
        /// 时间戳
        /// </summary>
        public const string Timespan = "timespan";

        public const string Role = "Role";
		public const string Subject = "Subject";
        public const string Name = "Name";
        public const string GivenName = "GivenName";
        public const string FamilyName = "FamilyName";
        public const string Email = "Email";
    }
}
