using Bi.Core.Models;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.ReportShares;
/// <summary>
/// 报表分享
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class ReportSharesController : BaseController 
{
    /// <summary>
    /// 报表分享 服务接口
    /// </summary>
    private readonly IReportSharesServices service;


    /// <summary>
    /// 报表分享 构造函数
    /// </summary>
    public ReportSharesController(IReportSharesServices service) {
        this.service = service;
    }

    /*/// <summary>
    /// 报表分享  列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<IEnumerable<ReportShare>>> getPagelist(ReportShare input) {
        return Success(await service.getEntityListAsync(input));
    }*/
}

