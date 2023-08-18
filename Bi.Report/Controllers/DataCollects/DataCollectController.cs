using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Bi.Core.Const;

namespace Bi.Report.Controllers.DataCollects;
/// <summary>
/// datasource 操作接口
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class DataCollectController : BaseController {

    /// <summary>
    /// 日志
    /// </summary>
    private ILogger<DataCollectController> logger;

    /// <summary>
    /// DataCollect 服务接口
    /// </summary>
    private readonly IDataCollectServices service;

    


    /// <summary>
    /// DataCollect 构造函数
    /// </summary>
    public DataCollectController(IDataCollectServices service,
                                ILogger<DataCollectController> logger)
    {
        this.service = service;
        this.logger = logger;
    }

    /// <summary>
    /// DataCollect 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(DataCollectInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.addAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success(result.ToString());
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 修改 DataCollect 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(DataCollectInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.ModifyAsync(input);
        if (result > 0)
            return Success(result.ToString());
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 删除 DataCollect 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(DataCollectDelete input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.deleteAsync(input);
        if (result > 0)
            return Success("删除成功！：共删除" + result.ToString() + "个");
        else
            return Error(result.ToString());
    }



    /// <summary>
    /// DataCollect 查询所有数据源
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("queryAllDataSource")]
    public async Task<ResponseResult<IEnumerable<DataSourceName>>> queryAllDataSource(DataSourceName input) {
        return Success(await service.getAllDataSource(input));
    }

    

    /// <summary>
    /// DataCollect  列表
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<DataCollect>>>> getPagelist(PageEntity<DataCollectInput> inputs) {
        return Success(await service.getEntityListAsync(inputs));
    }

    /// <summary>
    /// DataCollect  列表无caseresult
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPagelistNoCaseresult")]
    public async Task<ResponseResult<PageEntity<IEnumerable<DataCollect>>>> getPagelistNoCaseresult(PageEntity<DataCollectInput> inputs)
    {
        var res = await service.getEntityListAsync(inputs);
        foreach(var entity in res.Data)
        {
            entity.CaseResult = null;
            entity.DynSentence = null;
        }
        return Success(res);
    }

    /// <summary>
    /// DataCollect  列表只包含数据集名称
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getCodeList")]
    public async Task<ResponseResult<PageEntity<IEnumerable<Object>>>> getCodeList(PageEntity<DataCollectInput> inputs)
    {
        string like = inputs.Data.SetName?.Replace("∞", "")??"";
        inputs.Data.SetName = null;
        var res = await service.getEntityListAsync(inputs);
        IEnumerable<DataCollect> data = res.Data;
        if (like!="")
        {
            data = data.Where(x => x.SetName.IndexOf(like) != -1);
        }
        var objs = data.Select(x=>new {x.SetCode,x.SetName});
        return Success(new PageEntity<IEnumerable<object>>
        {
            Ascending = res.Ascending,
            OrderField = res.OrderField,
            PageIndex = res.PageIndex,
            PageSize = res.PageSize,
            Total = res.Total,
            Data = objs,
        });
    }

    /// <summary>
    /// DataCollect  单个数据集的第一个值
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getFirstValues")]
    public async Task<ResponseResult<PageEntity<IEnumerable<object>>>> getFirstValues(PageEntity<DataCollectInput> inputs)
    {
        return Success(await service.getFirstValues(inputs));
    }

    /// <summary>
    /// DataCollect  单个信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getDetailBysetCode")]
    public async Task<ResponseResult<DataCollectResponse>> getDetailBysetCode(DataCollectInput input) {
        return Success(await service.getDetailBysetCode(input));
    }

    

    /// <summary>
    /// DataCollect  列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("testTransform")]
    public async Task<ResponseResult<String>> testTransform(DataCollectInput input) {
        logger.LogInformation($"[{DateTime.Now}]testTransform开始执行！");
        return Success((await service.testTransform(input)).Item1);
    }
}

