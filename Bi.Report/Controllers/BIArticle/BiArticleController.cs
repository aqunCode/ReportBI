using Bi.Core.Models;
using Bi.Core.Const;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.BIArticle;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/12/14 10:38:03
/// 版本：1.1
/// </summary>

[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class BiArticleController:BaseController
{
    /// <summary>
    /// BiArticle 服务接口
    /// </summary>
    private readonly IBiArticleServices service;

    /// <summary>
    /// BiArticle 构造函数
    /// </summary>
    public BiArticleController(IBiArticleServices service)
    {
        this.service = service;
    }

    /// <summary>
    /// BiArticle 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insertAsync(BiArticleInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.addAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success("插入成功！");
        else if(result == BaseErrorCode.PleaseDoNotAddAgain)
            return Error("重复插入！", result);
        else
            return Error("插入失败！",result);
    }

    /// <summary>
    /// 删除 BiArticle 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(BiArticleInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.deleteAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success("删除成功！");
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 更新 BiArticle 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(BiArticleInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.ModifyAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success(result.ToString());
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// BiArticle  列表
    /// </summary>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<BiArticle>>>> getPagelist(PageEntity<BiArticleInput> inputs)
    {
        return Success(await service.getPagelist(inputs));
    }

    /// <summary>
    /// BiArticle  查询全部列表
    /// </summary>
    [HttpPost]
    [ActionName("getList")]
    public async Task<ResponseResult<IEnumerable<BiArticle>>> getList(BiArticleInput input)
    {
        return Success(await service.getList(input));
    }

    /// <summary>
    /// 获取 BiArticle  单个
    /// </summary>
    [HttpPost]
    [ActionName("getEntity")]
    public async Task<ResponseResult<BiArticle>> getEntity(BiArticleInput input)
    {
        var result = await service.getEntity(input);
        if(result.Item1 == BaseErrorCode.Successful)
        {
            return Success(result.Item2);
        }
        else
        {
            return Error("查询失败", result.Item2);
        }
        
    }
    
}

