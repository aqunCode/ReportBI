using Bi.Core.Helpers;
using MessagePack;
using System;

namespace Bi.Core.Models
{
    using Bi.Core.Extensions;
    using global::SqlSugar;

    /// <summary>
    /// 数据库表实体抽象基类
    /// </summary>
    [MessagePackObject(true)]
    public abstract class BaseEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key("ID")]
        [SugarColumn(IsPrimaryKey = true)]
        [ExcelColumn(IsExport = false)]
        public string Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [ExcelColumn("创建时间", Format = "date@yyyy-MM-dd HH:mm:ss")]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [ExcelColumn(IsExport = false)]
        public string CreateUserId { get; set; }

        /// <summary>
        /// 创建人名称
        /// </summary>
        [ExcelColumn("创建人")]
        public string CreateUserName { get; set; }

        /// <summary>
        /// 1 有效 0 无效
        /// </summary>
        [ExcelColumn("是否有效", Format = "dic@0:N,1:Y")]
        public int? Enabled { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [ExcelColumn("修改时间", Format = "date@yyyy-MM-dd HH:mm:ss")]
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// 修改人id
        /// </summary>
        [ExcelColumn(IsExport = false)]
        public string ModifyUserId { get; set; }

        /// <summary>
        /// 修改人名称
        /// </summary>
        [ExcelColumn("修改人")]
        public string ModifyUserName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [ExcelColumn("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="user">当前操作者</param>
        /// <param name="id">自定义主键，默认null，内部自动生成雪花ID</param>
        public virtual BaseEntity Create(CurrentUser user, string id = null)
        {
            if (!id.IsNullOrEmpty())
                this.Id = id;
            else
                this.Id = Sys.Guid;

            this.CreateDate = DateTimeExtensions.Now();
            this.CreateUserId = user.Account;
            this.CreateUserName = user.Name;
            this.Enabled = 1;

            return this;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="id">主键id</param>
        /// <param name="user">当前操作者</param>
        public virtual BaseEntity Modify(string id, CurrentUser user)
        {
            this.Id = id;
            this.ModifyDate = DateTimeExtensions.Now();
            this.ModifyUserId = user.Account;
            this.ModifyUserName = user.Name;

            return this;
        }
    }
}
