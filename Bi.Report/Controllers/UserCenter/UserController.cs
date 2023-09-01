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
    /// ��ȡ�û�ͷ��
    /// </summary>
    /// <param name="fileName">ͼƬ����</param>
    /// <returns></returns>
    [HttpGet]
    [ActionName("getheadicon")]
    public async Task<IActionResult> getHeadIconAsync(string fileName)
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
    }

    /// <summary>
    /// ����û���Ϣ
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult<string>> insert(UserInput input)
    {
        //��ȡͼƬ�ֽ���
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.insert(input);
        if (code  ==  BaseErrorCode.Successful)
            return Success("�û���ӳɹ�����ʼ����123456��");
        return Error();
    }

    /// <summary>
    /// ɾ���û���Ϣ
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult<string>> delete(UserInput input)
    {
        //��ȡͼƬ�ֽ���
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.delete(input);
        if (code == BaseErrorCode.Successful)
            return Success("ɾ���ɹ���");
        return Error();
    }

    /// <summary>
    /// ����û���Ϣ
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult<string>> modify(UserInput input)
    {
        //��ȡͼƬ�ֽ���
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.modify(input);
        if (code == BaseErrorCode.Successful)
            return Success("�޸ĳɹ���");
        return Error();
    }

    /// <summary>
    /// ��ѯ�����û���Ϣ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPageList")]
    public async Task<ResponseResult<PageEntity<IEnumerable<CurrentUser>>>> getPageList(PageEntity<UserInput> input)
    {
        //��ȡͼƬ�ֽ���
        var result = await userServices.getPageList(input);
        return Success(result);
    }

    /// <summary>
    /// ��ӽ�ɫ��Ϣ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("roleInsert")]
    public async Task<ResponseResult<string>> roleInsert(RoleAuthorizeInput input)
    {
        //��ȡͼƬ�ֽ��� 
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.roleInsert(input);
        if (code == BaseErrorCode.Successful)
            return Success("��ӽ�ɫ�ɹ�!");
        return Error();
    }

    /// <summary>
    /// ɾ����ɫ��Ϣ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("roleDelete")]
    public async Task<ResponseResult<string>> roleDelete(RoleAuthorizeInput input)
    {
        //��ȡͼƬ�ֽ���
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.roleDelete(input);
        if (code == BaseErrorCode.Successful)
            return Success("ɾ���ɹ���");
        return Error();
    }

    /// <summary>
    /// �޸Ľ�ɫ��Ϣ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("roleModify")]
    public async Task<ResponseResult<string>> roleModify(RoleAuthorizeInput input)
    {
        //��ȡͼƬ�ֽ���
        input.CurrentUser = this.CurrentUser;
        var code = await userServices.roleModify(input);
        if (code == BaseErrorCode.Successful)
            return Success("�޸ĳɹ���");
        return Error();
    }

    /// <summary>
    /// ��ѯ���н�ɫ��Ϣ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getRolePageList")]
    public async Task<ResponseResult<PageEntity<IEnumerable<RoleAuthorizeEntity>>>> getRolePageList(PageEntity<RoleAuthorizeInput> input)
    {
        //��ȡͼƬ�ֽ���
        var result = await userServices.getRolePageList(input);
        return Success(result);
    }
    /// <summary>
    /// ��ѯ���н�ɫid�б�
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ActionName("getRoleList")]
    public async Task<ResponseResult<IEnumerable<JObject>>> getRoleList()
    {
        //��ȡͼƬ�ֽ���
        var result = await userServices.getRoleList();
        var res = result.Select(x => new JObject(new JProperty("roleId", x.RoleId), new JProperty("roleName", x.RoleName)));
        return Success(res);
    }
}

