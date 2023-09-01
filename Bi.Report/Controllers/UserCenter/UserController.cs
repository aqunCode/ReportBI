using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Entity;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IServicep;
using Bi.Entities.Response;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using System.Net;
using Bi.Core.Const;
using Newtonsoft.Json.Linq;

namespace Bi.Report.Controllers.UserCenter;

/// <summary>
/// 用户管理中心
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class UserController : BaseController
{

    /// <summary>
    /// 首页 服务接口
    /// </summary>
    private readonly IUserService userServices;


    /// <summary>
    /// 构造函数
    /// </summary>
    public UserController(IUserService userServices)
    {
        this.userServices = userServices;
    }

    /// <summary>
    /// 获取用户ip信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ActionName("getip")]
    public ResponseResult<string> GetIp()
    {
        var ip = DnsHelper.GetClientRemoteIpAddress();

        if (ip.IsNullOrEmpty() || IPAddress.IsLoopback(IPAddress.Parse(ip)))
        {
            ip = DnsHelper.GetIpAddress(true, false);
            if (ip.IsNullOrEmpty())
                ip = DnsHelper.GetIpAddress(true, true);
        }

        return Success(data: ip);
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getcurrentuser")]
    public async Task<ResponseResult<CurrentUserResponse>> getCurrentUserAsync()
    {
        var currentUser = this.CurrentUser;
        var user = await userServices.GetEntityAsync(new UserQueryInput { Id = CurrentUser.Id, Enabled = -1 });
        currentUser.LastPasswordChangeTime = user.LastPasswordChangeTime;

        var retval = currentUser.MapTo<CurrentUserResponse>();
        if (currentUser.LastPasswordChangeTime != null)
        {
            retval.NeedChangePassword = (System.DateTime.Now - currentUser.LastPasswordChangeTime.Value).TotalDays
                                        >
                                        3 * 30;
        }
        else
        {
            retval.NeedChangePassword = true;
        }

        retval.VipLevel = await userServices.GetAndSetVipLevel(currentUser.Id);
        return this.Success(retval);
    }

    /// <summary>
    /// 获取用户头像
    /// </summary>
    /// <param name="fileName">图片名称</param>
    /// <returns></returns>
    [HttpGet]
    [ActionName("getheadicon")]
    public async Task<IActionResult> getHeadIconAsync(string fileName)
    {
        //获取图片字节码
        var (data, pictureBytes) = await userServices.GetPictureAsync(fileName);
        if (pictureBytes == null || pictureBytes.Length == 0)
            return NotFound();

        //获取文件ContentType
        var contentType = fileName.GetContentType();
        if (contentType.IsNullOrEmpty())
            return BadRequest();

        return File(pictureBytes, contentType, data);
    }

    /// <summary>
    /// 添加用户信息
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult<string>> insert(UserInput input)
    {
        //获取图片字节码
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.insert(input);
        if (code  ==  BaseErrorCode.Successful)
            return Success("用户添加成功！初始密码123456！");
        return Error();
    }

    /// <summary>
    /// 删除用户信息
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult<string>> delete(UserInput input)
    {
        //获取图片字节码
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.delete(input);
        if (code == BaseErrorCode.Successful)
            return Success("删除成功！");
        return Error();
    }

    /// <summary>
    /// 添加用户信息
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult<string>> modify(UserInput input)
    {
        //获取图片字节码
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.modify(input);
        if (code == BaseErrorCode.Successful)
            return Success("修改成功！");
        return Error();
    }

    /// <summary>
    /// 查询所有用户信息
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPageList")]
    public async Task<ResponseResult<PageEntity<IEnumerable<CurrentUser>>>> getPageList(PageEntity<UserInput> input)
    {
        //获取图片字节码
        var result = await userServices.getPageList(input);
        return Success(result);
    }

    /// <summary>
    /// 添加角色信息
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("roleInsert")]
    public async Task<ResponseResult<string>> roleInsert(RoleAuthorizeInput input)
    {
        //获取图片字节码 
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.roleInsert(input);
        if (code == BaseErrorCode.Successful)
            return Success("添加角色成功!");
        return Error();
    }

    /// <summary>
    /// 删除角色信息
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("roleDelete")]
    public async Task<ResponseResult<string>> roleDelete(RoleAuthorizeInput input)
    {
        //获取图片字节码
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.roleDelete(input);
        if (code == BaseErrorCode.Successful)
            return Success("删除成功！");
        return Error();
    }

    /// <summary>
    /// 修改角色信息
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("roleModify")]
    public async Task<ResponseResult<string>> roleModify(RoleAuthorizeInput input)
    {
        //获取图片字节码
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.roleModify(input);
        if (code == BaseErrorCode.Successful)
            return Success("修改成功！");
        return Error();
    }

    /// <summary>
    /// 查询所有角色信息
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getRolePageList")]
    public async Task<ResponseResult<PageEntity<IEnumerable<RoleAuthorizeEntity>>>> getRolePageList(PageEntity<RoleAuthorizeInput> input)
    {
        //获取图片字节码
        var result = await userServices.getRolePageList(input);
        return Success(result);
    }
    /// <summary>
    /// 查询所有角色id列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ActionName("getRoleList")]
    public async Task<ResponseResult<IEnumerable<JObject>>> getRoleList()
    {
        //获取图片字节码
        var result = await userServices.getRoleList();
        var res = result.Select(x => new JObject(new JProperty("roleId", x.RoleId), new JProperty("roleName", x.RoleName)));
        return Success(res);
    }
}

