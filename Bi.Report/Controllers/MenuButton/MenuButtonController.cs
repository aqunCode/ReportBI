using Bi.Core.Models;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IService;
using Bi.Entities.Response;
using Bi.Core.Extensions;

namespace Bi.Report.Controllers.MenuButton;

/// <summary>
/// �˵�����ť����
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class MenuButtonController : BaseController
{
    /// <summary>
    /// ˽���ֶ�
    /// </summary>
    private readonly IMenuButtonService menuButtonService;

    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="menuButtonService"></param>
    public MenuButtonController(IMenuButtonService menuButtonService)
    {
        this.menuButtonService = menuButtonService;
    }

    /// <summary>
    /// ��ȡ��ǰ�˻���Ȩ�˵�����ť��Ϣ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getlisttreecurrentuser")]
    public async Task<ResponseResult<IEnumerable<AuthMenuResponse>>> GetListTreeCurrentUserAsync()
    {
        var data = await menuButtonService.GetListTreeCurrentUserAsync(this.CurrentUser);
        if (data?.Count() > 0)
        {
            data = data
                    .TreeToJson("Id", childName: "children")
                    .ToObject<IEnumerable<AuthMenuResponse>>()
                     .OrderBy(x => x.SortCode);
        }
        return Success(data);
    }


}