using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.DataSources;

/// <summary>
/// datasource 操作接口
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class DataSourceController :BaseController {

    /// <summary>
    /// datasource 服务接口
    /// </summary>
    private readonly IDataSourceServices service;


    /// <summary>
    /// datasource 构造函数
    /// </summary>
    public DataSourceController(IDataSourceServices service) {
        this.service = service;
    }

    /// <summary>
    /// datasource 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(DataSourceInput input) {
        input.CurrentUser = this.CurrentUser;
        var result = await service.addAsync(input);
        if(result > 0)
            return Success();
        else
            return Error();
    }

    /// <summary>
    /// datasource  列表
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<DataSource>>>> getPagelist(PageEntity<DataSourceInput> inputs) {
        var res = await service.getEntityListAsync(inputs);
        return Success(res);
    }

    /// <summary>
    /// 修改 datasource 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(DataSourceInput input) {
        input.CurrentUser = this.CurrentUser;
        var result = await service.ModifyAsync(input);
        if(result > 0)
            return Success();
        else
            return Error();
    }

    /// <summary>
    /// 删除 datasource 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(DataSourceDelete input) {
        input.CurrentUser = this.CurrentUser;
        var result = await service.deleteAsync(input);
        if(result > 0)
            return Success("删除执行成功，共删除"+result+"个");
        else
            return Error("删除失败！");
    }

    /// <summary>
    /// 测试 datasource 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("testConnection")]
    public async Task<ResponseResult> testConnection(DataSourceInput input) {
        input.CurrentUser = this.CurrentUser;
        var result = await service.testConnection(input);
        if(result > 0)
            return Success("连接成功！");
        else
            return Error("连接失败！");
    }

}

