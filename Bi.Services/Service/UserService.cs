using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Polly.Caching;
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
        user.LastPasswordChangeTime = DateTime.Now;
        // 默认头像 小趴菜
        user.HeadIcon = "coin.png";
        await repository.Insertable<CurrentUser>(user).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> delete(UserInput input)
    {
        List<CurrentUser> list = new();
        if(input.MultiId != null)
        {
            foreach(var item in input.MultiId)
                list.Add( new CurrentUser { Id = item });
        }
        await repository.Deleteable<CurrentUser>(list).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> modify(UserInput input)
    {
        if (string.IsNullOrEmpty(input.Id))
            return BaseErrorCode.Fail;
        CurrentUser user = new();
        repository.Tracking(user);
        input.MapTo<UserInput,CurrentUser>(user);
        user.Modify(input.Id,input.CurrentUser);
        await repository.Updateable<CurrentUser>(user).ExecuteCommandAsync();
        repository.TempItems.Clear();
        return BaseErrorCode.Successful;
    }

    public async Task<PageEntity<IEnumerable<CurrentUser>>> getPageList(PageEntity<UserInput> input)
    {
        var data = await repository.Queryable<CurrentUser>()
                    .WhereIF(
                            input.Data.Account.IsNotNullOrEmpty()
                            , x => x.Account.Contains(input.Data.Account))
                    .WhereIF(
                            input.Data.Name.IsNotNullOrEmpty()
                            , x => x.Name.Contains(input.Data.Name))
                    .ToListAsync();
        data = data.Where(x => x.Account != "admin").ToList();
        return new PageEntity<IEnumerable<CurrentUser>>
        {
            PageIndex = input.PageIndex,
            Ascending = input.Ascending,
            PageSize = input.PageSize,
            OrderField = input.OrderField,
            Total = data.Count,
            Data = data
        };
    }

    public async Task<double> roleInsert(RoleAuthorizeInput input)
    {
        RoleAuthorizeEntity role = input.MapTo<RoleAuthorizeEntity>();
        role.Create(input.CurrentUser);
        await repository.Insertable<RoleAuthorizeEntity>(role).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> roleDelete(RoleAuthorizeInput input)
    {
        RoleAuthorizeEntity role = new RoleAuthorizeEntity { Id = input.Id };
        await repository.Deleteable<RoleAuthorizeEntity>(role).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> roleModify(RoleAuthorizeInput input)
    {
        if (string.IsNullOrEmpty(input.Id))
            return BaseErrorCode.Fail;
        RoleAuthorizeEntity role = input.MapTo<RoleAuthorizeEntity>();
        role.Modify(input.Id, input.CurrentUser);
        await repository.Updateable<RoleAuthorizeEntity>(role).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<PageEntity<IEnumerable<RoleAuthorizeEntity>>> getRolePageList(PageEntity<RoleAuthorizeInput> input)
    {
        var data = await repository.Queryable<RoleAuthorizeEntity>()
                    .WhereIF(
                            input.Data.RoleId.IsNotNullOrEmpty()
                            ,x=>x.RoleId.Contains(input.Data.RoleId))
                    .WhereIF(
                            input.Data.RoleName.IsNotNullOrEmpty()
                            ,x=>x.RoleName.Contains(input.Data.RoleName))
                    .ToListAsync();
        return new PageEntity<IEnumerable<RoleAuthorizeEntity>>
        {
            PageIndex = input.PageIndex,
            Ascending = input.Ascending,
            PageSize = input.PageSize,
            OrderField = input.OrderField,
            Total = data.Count,
            Data = data
        };
    }

    public async Task<IEnumerable<RoleAuthorizeEntity>> getRoleList()
    {
        return await repository.Queryable<RoleAuthorizeEntity>().ToListAsync();
    }
}
