using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Response;

public class CurrentUserResponse : CurrentUser
{
    /// <summary>
    /// 是否需要修改密码
    /// </summary>
    public bool NeedChangePassword { get; set; }

    /// <summary>
    /// Vip等级
    /// </summary>
    public int VipLevel { get; set; }
}
