using Bi.Core.Const;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.BIDataSetRelation;


[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class BIRelationController : BaseController
{
    /// <summary>
    /// ralation  关联服务接口
    /// </summary>
    private readonly IBIRelationServices service;

    /// <summary>
    /// ralation 构造函数
    /// </summary>
    public BIRelationController(IBIRelationServices service)
    {
        this.service = service;
    }

    /// <summary>
    /// ralation 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(BIRelationInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.addAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success(result.ToString());
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 删除 ralation 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(BIRelationInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.deleteAsync(input);
        if (result > 0)
            return Success("删除成功");
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 更新 ralation 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(BIRelationInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.ModifyAsync(input);
        if (result > 0)
            return Success(result.ToString());
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// ralation  列表
    /// </summary>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<BIRelation>>>> getPagelist(PageEntity<BIRelationInput> inputs)
    {
        return Success(await service.getPagelist(inputs));
    }


}
