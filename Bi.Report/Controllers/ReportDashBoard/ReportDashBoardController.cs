using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Bi.Report.Controllers.ReportDashBoard;
/// <summary>
/// 大屏报表 操作接口
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class ReportDashBoardController : BaseController 
{
    /// <summary>
    /// DashBoard 服务接口
    /// </summary>
    private readonly IReportDashBoardServices service;


    /// <summary>
    /// DashBoard 构造函数
    /// </summary>
    public ReportDashBoardController(IReportDashBoardServices service) 
    {
        this.service = service;
    }

    [HttpGet]
    [ActionName("preview")]
    public async Task<ResponseResult<DashBoardOutput>> preview(string reportCode)
    {
        DashBoardOutput result = await service.preview(reportCode);
        return Success<DashBoardOutput>(result);
    }

    /// <summary>
    /// DashBoard 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(DashBoardInput input) {
        input.CurrentUser = this.CurrentUser;
        var (res, msg)  = await service.insert(input);
        if (res)
            return Success(msg);
        return Error(msg);
    }

    [HttpPost]
    [ActionName("getData")]
    public  async Task<ResponseResult<JToken>> getData(ChartInput chartInput)
    {
        var result = await service.getChartData(chartInput);
        //此处开始数据二阶处理
        JToken ja = await service.turnData(chartInput.SetCode, result, chartInput.AutoTurn);
        if(ja != null) 
            return Success<JToken>(ja);
        return Error(ja);
    }

}

