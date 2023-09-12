using Bi.Entities.Entity;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IServicep;
using Bi.Entities.Response;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using System.Net;
using Bi.Core.Models;
using Bi.Entities.Input;
using System.ComponentModel.DataAnnotations;

namespace Bi.Report.Controllers.UserCenter;

/// <summary>
/// 数据字典明细管理
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class DataItemDetailController : BaseController
{
    /// <summary>
    /// 字段
    /// </summary>
    private readonly IDataItemDetailService _dataItemDetailService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dataItemDetailService"></param>
    public DataItemDetailController(IDataItemDetailService dataItemDetailService)
    {
        _dataItemDetailService = dataItemDetailService;
    }
    /// <summary>
    /// 新增数据字典明细
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insertTree")]
    public async Task<ResponseResult> insertTree(DataItemInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.insertTree(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }/// <summary>
     /// 删除数据字典明细
     /// </summary>
     /// <returns></returns>
    [HttpPost]
    [ActionName("deleteTree")]
    public async Task<ResponseResult> deleteTree(DataItemInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.deleteTree(input);
        if (result > 0)
            return Success("删除执行成功，共删除" + result + "个");
        else
            return Error("删除失败！");
    }/// <summary>
     /// 修改数据字典明细
     /// </summary>
     /// <returns></returns>
    [HttpPost]
    [ActionName("modifyTree")]
    public async Task<ResponseResult> modifyTree(DataItemInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.modifyTree(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }
    /// <summary>
    /// 新增数据字典明细
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(DataItemDetailQueryInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.insert(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }/// <summary>
     /// 删除数据字典明细
     /// </summary>
     /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> delete(DataItemDetailQueryInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.delete(input);
        if (result > 0)
            return Success("删除执行成功，共删除" + result + "个");
        else
            return Error("删除失败！");
    }/// <summary>
     /// 修改数据字典明细
     /// </summary>
     /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modify(DataItemDetailQueryInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.modify(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }


    /// <summary>
    /// 分页获取数据字典明细列表
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<DataItemDetailResponse>>>> getPagelist(PageEntity<DataItemDetailQueryInput> inputs)
    {
        var data = await _dataItemDetailService.getPagelist(inputs);
        return Success(data);
    }

    /// <summary>
    /// 获取数据字典明细列表
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getlist")]
    public async Task<ResponseResult<IEnumerable<DataItemDetailResponse>>> GetListAsync(DataItemDetailQueryInput input)
    {
        var data = await _dataItemDetailService.GetListAsync(input);
        return Success(data);
    }

    /// <summary>
    /// 获取数据字典树状结构
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpGet]
    [ActionName("getDataDictTree")]
    public async Task<ResponseResult<IEnumerable<DataItemTree>>> getDataDictTree()
    {
        var res = await _dataItemDetailService.getDataDictTree();
        if (res.Count() > 0)
        {
            res = res
                    .TreeToJson("Id", new[] { "0" }, childName: "children")
                    .ToObject<IEnumerable<DataItemTree>>();
        }
        return Success(res);
    }

}