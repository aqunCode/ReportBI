using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IServicep;
using Bi.Entities.Response;
using Bi.Core.Const;

namespace Bi.Report.Controllers.UserCenter;

/// <summary>
/// 首页
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class ErrorRecordController : BaseController
{

    /// <summary>
    /// 首页 服务接口
    /// </summary>
    private readonly IErrorRecordService errorRecordService;


    /// <summary>
    /// 构造函数
    /// </summary>
    public ErrorRecordController(IErrorRecordService errorRecordService)
    {
        this.errorRecordService = errorRecordService;
    }

    /// <summary>
    /// 表头 工作簿数量 访问次数 新增数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("add")]
    public async Task<ResponseResult<string>> insert(ErrorRecordInput input)
    {
        double result = await errorRecordService.insert(input);
        return Success(BaseCode.toChinesCode(result));
    }

}

