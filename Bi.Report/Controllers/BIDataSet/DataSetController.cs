using Bi.Core.Const;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.AutoReport.BIDataSet;

[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class BIDataSetController:BaseController
{
    /// <summary>
    /// DataSet 服务接口
    /// </summary>
    private readonly IDataSetServices service;

    /// <summary>
    /// DataSet 构造函数
    /// </summary>
    public BIDataSetController(IDataSetServices service)
    {
        this.service = service;
    }

    /// <summary>
    /// DataSet 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(DataSetInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.addAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success("插入成功！");
        else if(result == BaseErrorCode.PleaseDoNotAddAgain)
            return Error("重复插入！", result);
        else
            return Error("插入失败！",result);
    }

    /// <summary>
    /// 删除 DataSet 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(DataSetInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.deleteAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success("删除成功！");
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 更新 DataSet 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(DataSetInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.ModifyAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success(result.ToString());
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// DataSet  列表
    /// </summary>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<BiDataset>>>> getPagelist(PageEntity<DataSetInput> inputs)
    {
        return Success(await service.getPagelist(inputs));
    }

    /// <summary>
    /// DataSet  名称编码下拉框
    /// </summary>
    [HttpPost]
    [ActionName("getSelectlist")]
    public async Task<ResponseResult<IEnumerable<BiDataset>>> getSelectlist()
    {
        return Success(await service.getSelectlist());
    }

    /// <summary>
    /// 获取当前DB下的所有用户
    /// </summary>
    [HttpPost]
    [ActionName("getUserlist")]
    public async Task<ResponseResult<IEnumerable<String>>> getUserlist(TableInput input)
    {
        var value = await service.getUserlist(input);
        if (value.Item1 == "OK")
            return Success(value.Item2);
        else
            return new ResponseResult<IEnumerable<string>>()
            {
                Code = ResponseCode.Error,
                Message = value.Item1
            };
    }

    /// <summary>
    /// 获取当前DB用户下的所有表
    /// </summary>
    [HttpPost]
    [ActionName("getTablelist")]
    public async Task<ResponseResult<IEnumerable<String>>> getTablelist(TableInput input)
    {
        var value = await service.getTablelist(input);
        if (value.Item1 == "OK")
            return Success(value.Item2);
        else
            return new ResponseResult<IEnumerable<string>>()
            {
                Code = ResponseCode.Error,
                Message = value.Item1
            };
    }

    /// <summary>
    /// 获取当前DB用户下的表的所有字段
    /// </summary>
    [HttpPost]
    [ActionName("getColumnlist")]
    public async Task<ResponseResult<IEnumerable<ColumnInfo>>> getColumnlist(TableInput input)
    {
        var value = await service.getColumnlist(input);
        if (value.Item1 == "OK")
            return Success(value.Item2);
        else
            return new ResponseResult<IEnumerable<ColumnInfo>>()
            {
                Code = ResponseCode.Error,
                Message = value.Item1
            };
    }


}

