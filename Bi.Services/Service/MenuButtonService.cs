using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Response;
using Bi.Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

internal class MenuButtonService : IMenuButtonService
{
    public async Task<IEnumerable<AuthMenuResponse>> GetListTreeCurrentUserAsync(CurrentUser user)
    {
        List<AuthMenuResponse> authMenus = new ();
        authMenus.Add(new AuthMenuResponse
        {
            Id = "a",
            Name = "report",
            Title = "reportT",
            ParentId = "0",
            Href = "views\\bill-design-manage\\data-set.vue",
            Category = 1,
            Source = 1,
            Component = "",
            Icon = "",
            Apis = "",
            SortCode = 1
        });
        return authMenus;

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
        
    }
}
