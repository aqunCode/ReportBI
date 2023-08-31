using Bi.Core.Models;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IService;
using Bi.Entities.Response;
using Bi.Core.Extensions;
using Bi.Entities.Input;
using Bi.Entities.Entity;

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

    /// <summary>
    /// 分页获取菜单、按钮
    /// </summary>
    /// <param name="input">分页查询参数</param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getpagelisttree")]
    public async Task<ResponseResult<PageEntity<IEnumerable<MenuButtonResponse>>>> GetPageListTreeAsync(PageEntity<MenuButtonInput> input)
    {
        input.Data ??= new MenuButtonInput();
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

    /// <summary>
    /// 添加菜单信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(MenuButtonInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await menuButtonService.addAsync(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }

    /// <summary>
    /// 删除菜单信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(MenuButtonInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await menuButtonService.deleteAsync(input);
        if (result > 0)
            return Success("删除执行成功，共删除" + result + "个");
        else
            return Error("删除失败！");
    }
    
    /// <summary>
    /// 修改菜单信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(MenuButtonInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await menuButtonService.ModifyAsync(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }

    /// <summary>
    /// datasource  列表
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<MenuButtonEntity>>>> getPagelist(PageEntity<MenuButtonInput> inputs)
    {
        var res = await menuButtonService.getEntityListAsync(inputs);
        return Success(res);
    }

    /// <summary>
    /// 获取菜单按钮树状结构
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getMenuTree")]
    public async Task<ResponseResult<IEnumerable<MenuButtonTree>>> getMenuTree()
    {
        var res = await menuButtonService.getMenuTree();
        if (res.Count() > 0)
        {
            res = res
                    .TreeToJson("Id", new[] {  "0" }, childName: "children")
                    .ToObject<IEnumerable<MenuButtonTree>>();
        }
        return Success(res);
    }



}