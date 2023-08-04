using Bi.Core.Const;
using Bi.Core.Models;
using Bi.Entities;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Bi.Report.Controllers.BIWorkbooks;
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class BIWorkbookController:BaseController
{
    /// <summary>
    /// BIWorkbook 服务接口
    /// </summary>
    private readonly IBIWorkbookServices service;

    /// <summary>
    /// BiCollect 服务接口
    /// </summary>
    private readonly IBiCollectServices collectService;

    /// <summary>
    /// BIWorkbook 构造函数
    /// </summary>
    public BIWorkbookController(IBIWorkbookServices service, IBiCollectServices collectService)
    {
        this.service = service;
        this.collectService = collectService;
    }
    /// <summary>
    /// BIWorkbook 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(BIWorkbookInput input)
    {
        input.CurrentUser = this.CurrentUser;
            var result = await service.addAsync(input);
        if (result == "OK")
            return Success("插入成功");
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 删除 BIWorkbook 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(BIWorkbookInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.deleteAsync(input);
        if (result == "OK")
            return Success("删除成功！：共删除" +'1' + "个");
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 更新 BIWorkbook 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(BIWorkbookInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.ModifyAsync(input);
        if (result =="OK")
            return Success(result.ToString());
        else
            return Error(result.ToString());
    }
    /// <summary>
    /// 查询 BIWorkbook  列表
    /// </summary>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<BIWorkbook>>>> getPagelist(PageEntity<BIWorkbookInput> inputs)
    {
        return Success(await service.getEntityListAsync(inputs));
    }
    /// <summary>
    /// 编辑 BIWorkbook 进回显/预览
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getEcho")]
    public async Task<ResponseResult<BIWorkbookInput>> getEcho(BIWorkbookInput inputs)
    {
        var result = await service.getEchoAsync(inputs);
        if (result.Item1 == "OK")
            return Success(result.Item1, result.Item2);
        else
            return Error(result.Item1, result.Item2);
    }
    /// <summary>
    /// 预览查询 BIWorkbook 
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("preview")]
    public async Task<ResponseResult<DataTable>> preview(BIWorkbookInput inputs)
    {
        var result =  await service.previewAsync(inputs);
        if (result.Item1 == "OK")
            return Success(result.Item1, result.Item2);
        else
            return Error(result.Item1,result.Item2);
    }

    /// <summary>
    /// BiCollect 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insertCollect")]
    public async Task<ResponseResult> insertCollectAsync(BiCollectInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await collectService.addAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success("插入成功！");
        else if (result == BaseErrorCode.PleaseDoNotAddAgain)
            return Error("重复插入！", result);
        else
            return Error("插入失败！", result);
    }

    /// <summary>
    /// 删除 BiCollect 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("deleteCollect")]
    public async Task<ResponseResult> deleteCollectAsync(BiCollectInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await collectService.deleteAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success("删除成功！");
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// BiCollect  查询全部列表
    /// </summary>
    [HttpPost]
    [ActionName("getCollectList")]
    public async Task<ResponseResult<IEnumerable<BiCollect>>> getCollectList(BiCollectInput input)
    {
        input.UserId = this.CurrentUser.Account;
        return Success(await collectService.getList(input));
    }
}
