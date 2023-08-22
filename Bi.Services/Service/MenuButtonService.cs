using Bi.Core.Const;
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
        var userInfo = await repository.Queryable<CurrentUser>().FirstAsync(x => x.Account == user.Account);
        if (userInfo == null)
            return null; 
        string[] arr = userInfo.RoleIds.Split(',');
        var roles = await repository.Queryable<RoleAuthorizeEntity>().Where(x=> arr.Contains(x.RoleId)).ToListAsync();

        List<string> list = new();
        foreach(var role in roles)
        {
            list.AddRange(role.MenuButtonId.Split(','));
        }
        IEnumerable<string> enums = list.Distinct();

        var menus = await repository.Queryable<MenuButtonEntity>().Where(x => enums.Contains(x.Id)).ToListAsync();
        var fatherMenus = menus.Where(x => x.Category == 1);

        List<AuthMenuResponse> authMenus = new();
        foreach (var menu in fatherMenus)
        {
            AuthMenuResponse menuRes = menu.MapTo<AuthMenuResponse>();
            var chilMenus = menus.Where(x => x.ParentId == menu.Id);
            
            if (chilMenus.Any())
            {
                menuRes.Children = new List<AuthMenuResponse>();
                foreach (var chilMenu in chilMenus)
                {
                    menuRes.Children.Add(chilMenu.MapTo<AuthMenuResponse>());
                }
            }   
        }
        return authMenus;

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
        List<MenuButtonResponse> data = new();
        if(input.Data.ParentId == "3664875E390145FFA8422B80E8AE744B")
        {
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE700A6",
                Category = 2,
                Source = 1,
                Name = "sdf",
                Title = "新增",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-xinzeng",
                SortCode = 1,
                Apis = "add"
            });
            return new PageEntity<IEnumerable<MenuButtonResponse>>
            {
                PageIndex = input.PageIndex,
                Ascending = input.Ascending,
                PageSize = input.PageSize,
                OrderField = input.OrderField,
                Total = data.Count,
                Data = data
            };
        }

        if(input.Data.ParentId == "433599FB0E9542969DB2745268C5A290")
        {
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A02",
                Category = 2,
                Source = 1,
                Name = "attentionTemplate",
                Title = "考勤模型",
                ParentId = input.Data.ParentId,
                Href = "attentionTemplate",
                Component = "",
                Icon = "icon iconfont icon-xinzeng",
                SortCode = 1,
                Apis = "",
                Remark = "考勤模型,固定人员可以观看"
            });
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A03",
                Category = 2,
                Source = 1,
                Name = "machineTemplate",
                Title = "设备模型",
                ParentId = input.Data.ParentId,
                Href = "machineTemplate",
                Component = "",
                Icon = "icon iconfont icon-xinzeng",
                SortCode = 2,
                Apis = "",
                Remark = "设备模型,关于设备项目专案"
            });
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A04",
                Category = 2,
                Source = 1,
                Name = "add",
                Title = "新增",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-xinzeng",
                SortCode = 11,
                Apis = "add",
                Remark = "0"
            });
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A05",
                Category = 2,
                Source = 1,
                Name = "edit",
                Title = "修改",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-edit",
                SortCode = 1,
                Apis = "edit",
                Remark = "0"
            });
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A06",
                Category = 2,
                Source = 12,
                Name = "delete",
                Title = "删除",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-delete",
                SortCode = 13,
                Apis = "delete",
                Remark = "0"
            });


            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A07",
                Category = 2,
                Source = 1,
                Name = "add",
                Title = "新增",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-xinzeng",
                SortCode = 14,
                Apis = "add",
                Remark = "1"
            });
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A08",
                Category = 2,
                Source = 1,
                Name = "edit",
                Title = "修改",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-edit",
                SortCode = 15,
                Apis = "edit",
                Remark = "1"
            });
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A09",
                Category = 2,
                Source = 12,
                Name = "delete",
                Title = "删除",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-delete",
                SortCode = 16,
                Apis = "delete",
                Remark = "1"
            });


            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A10",
                Category = 2,
                Source = 1,
                Name = "add",
                Title = "新增",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-xinzeng",
                SortCode = 17,
                Apis = "add",
                Remark = "2"
            });
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A11",
                Category = 2,
                Source = 1,
                Name = "edit",
                Title = "修改",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-edit",
                SortCode = 18,
                Apis = "edit",
                Remark = "2"
            });
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE70A12",
                Category = 2,
                Source = 12,
                Name = "delete",
                Title = "删除",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-delete",
                SortCode = 19,
                Apis = "delete",
                Remark = "2"
            });
            return new PageEntity<IEnumerable<MenuButtonResponse>>
            {
                PageIndex = input.PageIndex,
                Ascending = input.Ascending,
                PageSize = input.PageSize,
                OrderField = input.OrderField,
                Total = data.Count,
                Data = data
            };
        }

        if (input.Data.ParentId != "0E78529EC9AF4487A1BC21FA871A6740")
        {
            data.Add(new MenuButtonResponse
            {
                Id = "B0195E68C53F4C1CB7AEFA411AE700A6",
                Category = 2,
                Source = 1,
                Name = "add",
                Title = "新增",
                ParentId = input.Data.ParentId,
                Href = null,
                Component = "",
                Icon = "icon iconfont icon-xinzeng",
                SortCode = 1,
                Apis = "add"
            });
        }
            

        data.Add(new MenuButtonResponse
        {
            Id = "B0195E68C53F4C1CB7AEFA411AE700A5",
            Category = 2,
            Source = 1,
            Name = "edit",
            Title = "修改",
            ParentId = input.Data.ParentId,
            Href = null,
            Component = "",
            Icon = "icon iconfont icon-edit",
            SortCode = 1,
            Apis = "edit"
        });
        data.Add(new MenuButtonResponse
        {
            Id = "B0195E68C53F4C1CB7AEFA411AE700A4",
            Category = 2,
            Source = 1,
            Name = "delete",
            Title = "删除",
            ParentId = input.Data.ParentId,
            Href = null,
            Component = "",
            Icon = "icon iconfont icon-delete",
            SortCode = 1,
            Apis = "delete"
        });
        
        //动态拼接Join条件

        return new PageEntity<IEnumerable<MenuButtonResponse>>
        {
            PageIndex = input.PageIndex,
            Ascending = input.Ascending,
            PageSize = input.PageSize,
            OrderField = input.OrderField,
            Total = data.Count,
            Data = data
        };
    }

    public Task<int> addAsync(MenuButtonInput input)
    {
        throw new NotImplementedException();
    }

    public Task<int> deleteAsync(MenuButtonInput input)
    {
        throw new NotImplementedException();
    }

    public Task<int> ModifyAsync(MenuButtonInput input)
    {
        throw new NotImplementedException();
    }
    public Task<PageEntity<IEnumerable<MenuButtonEntity>>> getEntityListAsync(PageEntity<MenuButtonInput> inputs)
    {
        throw new NotImplementedException();
    }
}
