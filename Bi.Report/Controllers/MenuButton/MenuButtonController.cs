using Bi.Core.Models;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IService;
using Bi.Entities.Response;
using Bi.Core.Extensions;

namespace Bi.Report.Controllers.MenuButton;

/// <summary>
/// 菜单、按钮管理
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class MenuButtonController : BaseController
{
    /// <summary>
    /// 私有字段
    /// </summary>
    private readonly IMenuButtonService menuButtonService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="menuButtonService"></param>
    public MenuButtonController(IMenuButtonService menuButtonService)
    {
        this.menuButtonService = menuButtonService;
    }

    /// <summary>
    /// 获取当前账户授权菜单、按钮信息
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