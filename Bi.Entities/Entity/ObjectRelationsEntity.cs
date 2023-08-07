using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Entity;

public class ObjectRelationsEntity : BaseEntity
{
    /// <summary>
    /// 1-菜单Id 2-角色Id 3-用户Id 4-公司/部门的Id 5-Api Id 6-数据库Id 7-区域/楼层/线体
    /// </summary>
    public string ObjectId { get; set; }

    /// <summary>
    /// 分类：1-菜单 2-角色 3-用户 4-公司/部门 5-Api 6-数据库 7-区域/楼层/线体
    /// </summary>
    public int? Category { get; set; }

    /// <summary>
    /// 系统标识
    /// </summary>
    public string SystemFlag { get; set; }
}
