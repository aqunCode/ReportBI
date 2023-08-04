using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Bi.Report.Controllers.BIcalculator;

[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class BiCalculatorController : BaseController
{
    /// <summary>
    /// DataSet 服务接口
    /// </summary>
    private readonly IBiCalculatorServices service;

    /// <summary>
    /// DataSet 构造函数
    /// </summary>
    public BiCalculatorController(IBiCalculatorServices service)
    {
        this.service = service;
    }

    /// <summary>
    /// 执行查询
    /// </summary>
    [HttpPost]
    [ActionName("execute")]
    public async Task<ResponseResult<DataTable>> execute(BIWorkbookInput input)
    {
        var result = await service.execute(input);
        if(result.Item1 == "OK")
            return Success(result.Item1,result.Item2);
        else
            return Error(result.Item1,result.Item2);
    }

    /// <summary>
    /// 获取字段所有值
    /// </summary>
    [HttpPost]
    [ActionName("selectValue")]
    public async Task<ResponseResult<PageEntity<List<string>>>> selectValue(PageEntity<ColumnInfo> input)
    {
        var res = await service.selectValue(input);

        if (res.Item1 == "OK")
            return Success(res.Item1, res.Item2);
        else
            return Error(res.Item1, res.Item2);


    }

    /// <summary>
    /// 获取标记字段所有值
    /// </summary>
    [HttpPost]
    [ActionName("getMarkValue")]
    public async Task<ResponseResult<List<string>>> getMarkValue(MarkValueInput input)
    {
        var res = await service.getValueList(input);

        if (res.Item1 == "OK")
            return Success(res.Item1, res.Item2);
        else
            return Error(res.Item1, res.Item2);

    }

    /// <summary>
    /// 获取标记字段所有值
    /// </summary>
    [HttpPost]
    [ActionName("sendWxMessage")]
    public async Task<ResponseResult<string>> sendWxMessage()
    {
        return Success(await service.sendWxMessage());
    }

}
