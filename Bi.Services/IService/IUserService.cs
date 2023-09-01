using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.IService;

public interface IUserService : IDependency
{
    Task<int> GetAndSetVipLevel(string id);
    Task<CurrentUser> GetEntityAsync(UserQueryInput userQueryInput);
    Task<(string fileName, byte[] datas)> GetPictureAsync(string fileName);
    /// <summary>
    /// 用户插入
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<double> insert(UserInput input);
    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<double> delete(UserInput input);
    /// <summary>
    /// 修改用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<double> modify(UserInput input);
    /// <summary>
    /// 查询所有用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PageEntity<IEnumerable<CurrentUser>>> getPageList(PageEntity<UserInput> input);
    Task<double> roleInsert(RoleAuthorizeInput input);
    Task<double> roleDelete(RoleAuthorizeInput input);
    Task<double> roleModify(RoleAuthorizeInput input);
    Task<PageEntity<IEnumerable<RoleAuthorizeEntity>>> getRolePageList(PageEntity<RoleAuthorizeInput> input);
    Task<IEnumerable<RoleAuthorizeEntity>> getRoleList();
}
