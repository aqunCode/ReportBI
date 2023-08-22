using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Services.IService;
using SharpCompress.Common;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

internal class UserService : IUserService
{
    /// <summary>
    /// 数据库链接
    /// </summary>
    private SqlSugarScopeProvider repository;

    public UserService(ISqlSugarClient _sqlSugarClient)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
    }

    public async Task<int> GetAndSetVipLevel(string id)
    {
        return 1;
    }

    public async Task<CurrentUser> GetEntityAsync(UserQueryInput userQueryInput)
    {
        CurrentUser user = await repository.Queryable<CurrentUser>().FirstAsync(x => x.Id == userQueryInput.Id);
        // 这里查询用户的菜单权限
        return user;
    }

    public async Task<(string fileName, byte[] datas)> GetPictureAsync(string fileName)
    {
        string rootPath = AppContext.BaseDirectory;
        rootPath = Path.Combine(rootPath, "picture",fileName);
        byte[] fileBytes = File.ReadAllBytes(rootPath);
        return (fileName, fileBytes);
    }

    public async Task<double> insert(UserInput input)
    {
        CurrentUser user = input.MapTo<CurrentUser>();
        user.Create(input.CurrentUser);
        await repository.Insertable<CurrentUser>(user).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> delete(UserInput input)
    {
        CurrentUser user = new CurrentUser{Id = input.Id};
        await repository.Deleteable<CurrentUser>(user).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> modify(UserInput input)
    {
        if (string.IsNullOrEmpty(input.Id))
            return BaseErrorCode.Fail;
        CurrentUser user = input.MapTo<CurrentUser>();
        user.Modify(input.Id,input.CurrentUser);
        await repository.Updateable<CurrentUser>(user).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }
}
