using Bi.Core.Models;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IService;
using Bi.Entities.Response;
using Bi.Core.Extensions;
using Bi.Entities.Input;

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

    /// <summary>
    /// ��ҳ��ȡ�˵�����ť
    /// </summary>
    /// <param name="input">��ҳ��ѯ����</param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getpagelisttree")]
    public async Task<ResponseResult<PageEntity<IEnumerable<MenuButtonResponse>>>> GetPageListTreeAsync(PageEntity<MenuButtonQueryInput> input)
    {
        input.Data ??= new MenuButtonQueryInput();
        input.Data.CurrentUser = this.CurrentUser;
        var result = await menuButtonService.GetPageListTreeAsync(input);
        if (result?.Data?.Count() > 0)
        {
            result.Data = result.Data
                            .TreeToJson("Id", new[] { input.Data.ParentId.IsNullOrEmpty() ? "0" : input.Data.ParentId }, childName: "children")
                            .ToObject<IEnumerable<MenuButtonResponse>>();
        }
        return Success(result);
    }

}