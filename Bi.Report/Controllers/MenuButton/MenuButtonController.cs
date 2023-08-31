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
    /// ��Ӳ˵���Ϣ
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
    /// ɾ���˵���Ϣ
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
            return Success("ɾ��ִ�гɹ�����ɾ��" + result + "��");
        else
            return Error("ɾ��ʧ�ܣ�");
    }
    
    /// <summary>
    /// �޸Ĳ˵���Ϣ
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
    /// datasource  �б�
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
    /// ��ȡ�˵���ť��״�ṹ
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