using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.DataDicts;
/// <summary>
/// 数据类型查询 接口
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class DataDictsController : BaseController {

    /// <summary>
    /// 数据类型查询 服务接口
    /// </summary>
    private readonly IDataDictServices service;


    /// <summary>
    /// 数据类型查询 构造函数
    /// </summary>
    public DataDictsController(IDataDictServices service) {
        this.service = service;
    }

    /// <summary>
    /// 数据类型查询  列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<IEnumerable<DataDict>>> getPagelist(DataDictInput input) {
        return Success(await service.getEntityListAsync(input));
    }
}

