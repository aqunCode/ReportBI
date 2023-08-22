using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IServicep;
using Bi.Entities.Response;

namespace Bi.Report.Controllers.UserCenter;

/// <summary>
/// 首页
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class ConnectController : BaseController
{

    /// <summary>
    /// 首页 服务接口
    /// </summary>
    private readonly IConnectService connectServices;


    /// <summary>
    /// 构造函数
    /// </summary>
    public ConnectController(IConnectService connectServices)
    {
        this.connectServices = connectServices;
    }

    /// <summary>
    /// 表头 工作簿数量 访问次数 新增数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("token")]
    public async Task<TokenResponse> getReportBiRecord([FromForm] UserInfo input)
    {
        return await connectServices.getToken(input);
    }

}

