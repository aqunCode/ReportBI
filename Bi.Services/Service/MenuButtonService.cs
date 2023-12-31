﻿using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

internal class MenuButtonService : IMenuButtonService
{
    /// <summary>
    /// 数据库链接
    /// </summary>
    private SqlSugarScopeProvider repository;

    public MenuButtonService(ISqlSugarClient _sqlSugarClient)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
    }

    /// <summary>
    /// 根据用户信息获取当前用户下的菜单权限
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<AuthMenuResponse>> GetListTreeCurrentUserAsync(CurrentUser user)
    {
        var userInfo = await repository.Queryable<CurrentUser>().FirstAsync(x => x.Account == user.Account && x.Enabled == 1);
        if (userInfo == null)
            return null;
        string[] arr = userInfo.RoleIds.Split(',');
        var roles = await repository.Queryable<RoleAuthorizeEntity>().Where(x=> arr.Contains(x.RoleId) && x.Enabled == 1).ToListAsync();

        List<string> list = new();
        foreach(var role in roles)
        {
            list.AddRange(role.MenuButtonId.Split(','));
        }
        IEnumerable<string> enums = list.Distinct();

        List<MenuButtonEntity> menus = new();
        if (AppSettings.IsAdministrator(userInfo.Account) == 1)
            menus = await repository.Queryable<MenuButtonEntity>().Where(x => x.Enabled == 1).ToListAsync();
        else
            menus = await repository.Queryable<MenuButtonEntity>().Where(x => enums.Contains(x.Id) && x.Enabled == 1).ToListAsync();

        var fatherMenus = menus.Where(x => x.Category == 1);

        List<AuthMenuResponse> authMenus = new();
        foreach (var menu in fatherMenus)
        {
            AuthMenuResponse menuRes = menu.MapTo<AuthMenuResponse>();
            authMenus.Add(menuRes);
        }
        return authMenus.OrderBy(x => x.SortCode).ToList();

        #region 原始写法注释
        /*//动态拼接Join条件

        var condition = LinqExtensions
                        .True<MenuButtonEntity, RoleAuthorizeEntity>()
                        .And((m, r) =>
                            r.MenuButtonId == m.Id &&
                            r.Enabled == 1);

        //判断当前用户是否系统管理员
        if (!AppSettings.IsAdministrator(user.Account))
        {
            //当前用户直接拥有角色及子角色
            var roleIds = (await _roleService.GetRolesAsync(user.RoleIds, user.SystemFlag)).Select(x => x.Id).Distinct();
            if (roleIds == null || roleIds.Count() == 0)
                return null;

            condition = condition.And((m, r) => roleIds.Contains(r.RoleId));
        }

        //当前系统所属角色和菜单
        var builder = SqlBuilder
                        .Select<MenuButtonEntity, RoleAuthorizeEntity, ObjectRelationsEntity, ObjectRelationsEntity>((m, r, o, o1) =>
                            new { m.Id, m.Name, m.Title, m.ParentId, m.Href, m.Component, m.Icon, m.Category, m.SortCode, m.Apis, m.Source },
                            _repository.DatabaseType,
                            isEnableFormat: false)
                        .InnerJoin<RoleAuthorizeEntity>(condition)
                        .InnerJoin<RoleAuthorizeEntity, ObjectRelationsEntity>((r, o) =>
                            o.ObjectId == r.RoleId &&
                            o.Enabled == 1 &&
                            o.Category == 2 &&
                            o.SystemFlag == user.SystemFlag)
                        .InnerJoin<ObjectRelationsEntity>((m, o1) =>
                            o1.ObjectId == m.Id &&
                            o1.Category == 1 &&
                            o1.Enabled == 1 &&
                            o1.SystemFlag == user.SystemFlag)
                        .Where(m =>
                            m.Enabled == 1 &&
                            m.Category == 1 &&
                            m.Source == user.Source)
                        .OrderBy(x => x.SortCode)
                        .Distinct();

        return await _repository.FindListAsync<AuthMenuResponse>(builder.Sql, builder.Parameters);*/
        #endregion
    }

    public async Task<PageEntity<IEnumerable<MenuButtonResponse>>> GetPageListTreeAsync(PageEntity<MenuButtonInput> input)
    {
        var userInfo = await repository.Queryable<CurrentUser>().FirstAsync(x => x.Account == input.Data.CurrentUser.Account && x.Enabled == 1);
        if (userInfo == null)
            return null;
        string[] arr = userInfo.RoleIds.Split(',');
        var roles = await repository.Queryable<RoleAuthorizeEntity>().Where(x => arr.Contains(x.RoleId) && x.Enabled == 1).ToListAsync();

        List<string> list = new();
        foreach (var role in roles)
        {
            list.AddRange(role.MenuButtonId.Split(','));
        }
        IEnumerable<string> enums = list.Distinct();

        List<MenuButtonEntity> menus = new();
        if (AppSettings.IsAdministrator(userInfo.Account) == 1)
            menus = await repository.Queryable<MenuButtonEntity>().Where(x =>  x.ParentId == input.Data.ParentId && x.Enabled == 1).ToListAsync();
        else
            menus = await repository.Queryable<MenuButtonEntity>().Where(x => enums.Contains(x.Id) && x.ParentId == input.Data.ParentId &&  x.Enabled == 1).ToListAsync();

        List<MenuButtonResponse> data = new();
        foreach(var button in menus)
        {
            data.Add(button.MapTo<MenuButtonResponse>());
        }
        return new PageEntity<IEnumerable<MenuButtonResponse>>
        {
            PageIndex = input.PageIndex,
            Ascending = input.Ascending,
            PageSize = input.PageSize,
            OrderField = input.OrderField,
            Total = data.Count,
            Data = data.OrderBy(x => x.SortCode).ToList()
        };
    }

    public async Task<double> addAsync(MenuButtonInput input)
    {
        MenuButtonEntity menu = input.MapTo<MenuButtonEntity>();
        menu.Create(input.CurrentUser);
        menu.Source = 1;
        await repository.Insertable<MenuButtonEntity>(menu).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> deleteAsync(MenuButtonInput input)
    {
        List<MenuButtonEntity> list = new();
        foreach(var item in input.multiId)
        {
            list.Add(new MenuButtonEntity { Id = item });
        }
        await repository.Deleteable<MenuButtonEntity>(list).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> ModifyAsync(MenuButtonInput input)
    {
        if (string.IsNullOrEmpty(input.Id))
            return BaseErrorCode.Fail;
        MenuButtonEntity menu = new();
        repository.Tracking(menu);
        input.MapTo<MenuButtonInput,MenuButtonEntity>(menu);
        menu.Modify(input.Id, input.CurrentUser);
        await repository.Updateable<MenuButtonEntity>(menu).ExecuteCommandAsync();
        repository.TempItems.Clear();
        return BaseErrorCode.Successful;
    }
    public async Task<PageEntity<IEnumerable<MenuButtonEntity>>> getEntityListAsync(PageEntity<MenuButtonInput> input)
    {
        var data = await repository.Queryable<MenuButtonEntity>()
                    .WhereIF(
                            input.Data.ParentId.IsNotNullOrEmpty()
                            , x => x.ParentId == input.Data.ParentId)
                    .WhereIF(
                            input.Data.Name.IsNotNullOrEmpty()
                            , x => x.Name.Contains(input.Data.Name))
                    .WhereIF(
                            input.Data.Category != 0
                            , x => x.Category == input.Data.Category)
                    .OrderBy(x=>x.SortCode)
                    .ToListAsync();
        return new PageEntity<IEnumerable<MenuButtonEntity>>
        {
            PageIndex = input.PageIndex,
            Ascending = input.Ascending,
            PageSize = input.PageSize,
            OrderField = input.OrderField,
            Total = data.Count,
            Data = data
        };
    }

    public async Task<IEnumerable<MenuButtonTree>> getMenuTree()
    {
        var res = await repository.Ado.SqlQueryAsync<MenuButtonTree>(" select id,parentid ,title   from bireport.dbo.sys_menu_button");
        return res;
    }
}
