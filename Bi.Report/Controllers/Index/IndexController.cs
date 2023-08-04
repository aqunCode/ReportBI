using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Entity;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.Index;

/// <summary>
/// 首页
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class IndexController : BaseController {

    /// <summary>
    /// 首页 服务接口
    /// </summary>
    private readonly IIndexService indexServices;


    /// <summary>
    /// 构造函数
    /// </summary>
    public IndexController(IIndexService indexServices) {
        this.indexServices = indexServices;
    }

    /// <summary>
    /// 表头 工作簿数量 访问次数 新增数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getReportBiRecord")]
    public async Task<ResponseResult<BiRecord>> getReportBiRecord(IndexInput input) {
        input.CurrentUser = CurrentUser;
        return Success(await indexServices.getRecord(input));
    }

    /// <summary>
    /// 获取访问次数最多top5
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getTopFive")]
    public async Task<ResponseResult<List<BiFrequency>>> getTopFive(IndexInput input)
    {
        input.CurrentUser = CurrentUser;
        return Success(await indexServices.getTopFive(input));
    }

    /// <summary>
    /// 获取单个的访问次数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getTopChartRecord")]
    public async Task<ResponseResult<List<BiChartRecord>>> getTopChartRecord(IndexInput input)
    {
        input.CurrentUser = CurrentUser;
        return Success(await indexServices.getTopChartRecord(input));
    }

    /// <summary>
    /// 获取单个的访问次数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getModelRecord")]
    public async Task<ResponseResult<List<BiModelRecord>>> getModelRecord(IndexInput input)
    {
        input.CurrentUser = CurrentUser;
        return Success(await indexServices.getModelRecord(input));
    }

    /// <summary>
    /// 获取单个的访问次数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getUserCollect")]
    public async Task<ResponseResult<List<BiModelRecord>>> getUserCollect(IndexInput input)
    {
        input.CurrentUser = CurrentUser;
        return Success(await indexServices.getModelRecord(input));
    }

}

