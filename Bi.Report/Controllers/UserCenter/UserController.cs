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

namespace Bi.Report.Controllers.UserCenter;

/// <summary>
/// �û���������
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class UserController : BaseController
{

    /// <summary>
    /// ��ҳ ����ӿ�
    /// </summary>
    private readonly IUserService userServices;


    /// <summary>
    /// ���캯��
    /// </summary>
    public UserController(IUserService userServices)
    {
        this.userServices = userServices;
    }

    /// <summary>
    /// ��ȡ�û�ip��Ϣ
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
    /// ��ȡ��ǰ�û���Ϣ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getcurrentuser")]
    public async Task<ResponseResult<CurrentUserResponse>> GetCurrentUserAsync()
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

    /*/// <summary>
    /// ��ȡ�û�ͷ��
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpGet]
    [ActionName("getheadicon")]
    public async Task<IActionResult> GetHeadIconAsync(string fileName)
    {
        //��ȡͼƬ�ֽ���
        var (data, pictureBytes) = await userServices.GetPictureAsync(fileName);
        if (pictureBytes == null || pictureBytes.Length == 0)
            return NotFound();

        //��ȡ�ļ�ContentType
        var contentType = fileName.GetContentType();
        if (contentType.IsNullOrEmpty())
            return BadRequest();

        return File(pictureBytes, contentType, data);
    }*/
}

