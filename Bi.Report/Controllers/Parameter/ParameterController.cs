using Bi.Core.Models;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Baize.Report.Controllers.Parameter;

[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class ParameterController : BaseController
{

    /// <summary>
    /// ftp 服务接口
    /// </summary>
    private readonly IParameterService services;


    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="parameterService"></param>
    public ParameterController(IParameterService parameterService)
    {
        this.services = parameterService;
    }
}