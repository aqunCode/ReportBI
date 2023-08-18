using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Services.IService;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

internal class UserService : IUserService
{
    public async Task<int> GetAndSetVipLevel(string id)
    {
        return 1;
    }

    public async Task<CurrentUser> GetEntityAsync(UserQueryInput userQueryInput)
    {
        // 假装查询了数据库
        CurrentUser user = new CurrentUser
        {
            Account = "2542",
            Id = userQueryInput.Id,
            Name = "葛鹏飞",
            Email = "1.qq.com",
            SystemFlag = "Y",
            RoleIds = "a,b,c,d,e",
            CompanyIds = "hostar",
            DepartmentIds = "it",
            HeadIcon = "coin.jpg",
            LastPasswordChangeTime = DateTime.Now,
            Source = 1
        };
        return user;
    }

    public async Task<(string fileName, byte[] datas)> GetPictureAsync(string fileName)
    {
        string rootPath = AppContext.BaseDirectory;
        rootPath = Path.Combine(rootPath, "picture",fileName);
        byte[] fileBytes = File.ReadAllBytes(rootPath);
        return (fileName, fileBytes);
    }
}
