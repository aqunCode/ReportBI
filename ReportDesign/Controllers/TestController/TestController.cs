using Bi.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ReportDesign.Controllers.TestController;

[ApiController]
[ApiVersion("1")]
[Route("api/autoreportcenter/anonymous/v{version:apiVersion}/[controller]/[action]")]
public class TestController : Controller
{
    /// <summary>
    /// DataCollect 服务接口
    /// </summary>
    private readonly ITestServices service;

    public TestController(ITestServices service)
    {
        this.service = service;
    }


    /// <summary>
    /// DataCollect 查询所有数据源
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("queryAll")]
    public async Task<DataTable> queryAllDataSource()
    {
        return await service.queryAll();
    }
}

