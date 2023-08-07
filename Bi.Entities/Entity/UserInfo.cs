using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Entity;

/// <summary>
/// 描述：用于用户登录
/// 作者：GPF
/// 创建日期：2023/8/3 13:41:00
/// 版本：1.1
/// </summary>
public class UserInfo
{
    public string? Client_id { set; get; }
    public string? Client_secret { set; get; }
    public string? Grant_type { set; get; }
    public string? Username { set; get; }
    public string? Password { set; get; }
    public string? Scope { set; get; }
    public string? Source { set; get; }
}
