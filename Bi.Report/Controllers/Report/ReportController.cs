using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Baize.Report.Controllers.Report;

/// <summary>
/// report 操作接口
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class ReportController : BaseController
{
    /// <summary>
    /// datasource 服务接口
    /// </summary>
    private readonly IReportService service;


    /// <summary>
    /// datasource 构造函数
    /// </summary>
    public ReportController(IReportService service)
    {
        this.service = service;
    }

    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(ReportInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var (res, msg) = await service.AddAsync(new[] { input });
        if (res)
            return Success(msg);

        return Error(msg);
        //if (result > 0)
        //    return Success();
        //else
        //    return Error();
    }

    [HttpPost]
    [ActionName("list")]
    public async Task<IEnumerable<AutoReport>> List(ReportQueryInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.GetEntityListAsync(input);
        return result;
    }

    [HttpPost]
    [ActionName("getpagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<AutoReport>>>> GetPageListAsync(PageEntity<ReportQueryInput> input)
    {
        var res = await service.GetPageListAsync(input);
        return Success(res);
    }

    /// <summary>
    /// 修改 report 信息
    /// </summary>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(ReportInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var (res, msg) = await service.ModifyAsync(input);
        //if (result > 0)
        //    return Success();
        //else
        //    return Error();
        if (res)
            return Success(msg);

        return Error(msg);
    }

    /// <summary>
    /// 删除 report 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(ReportQueryInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.deleteAsync(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }

    [HttpPost]
    [ActionName("batchDelete")]
    public async Task<ResponseResult> batchDeleteAsync(string[] ids)
    {
        await service.DeleteAsync(ids, this.CurrentUser);
        return Success("删除成功");
    }


}

