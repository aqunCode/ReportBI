using Bi.Core.Const;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.BIWorkbooks;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2022/12/28 17:01:12
/// 版本：1.0
/// </summary>

[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class BiCustomerFieldController:BaseController
{
    /// <summary>
    /// BiCustomerField 服务接口
    /// </summary>
    private readonly IBiCustomerFieldServices service;

    /// <summary>
    /// BiCustomerField 构造函数
    /// </summary>
    public BiCustomerFieldController(IBiCustomerFieldServices service)
    {
        this.service = service;
    }

    /// <summary>
    /// BiCustomerField 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(BiCustomerFieldInput input)
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
    /// 删除 BiCustomerField 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> deleteAsync(BiCustomerFieldInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.deleteAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success("删除成功！");
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// 更新 BiCustomerField 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(BiCustomerFieldInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.ModifyAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success("修改成功");
        else
            return Error(result.ToString());
    }

    /// <summary>
    /// BiCustomerField  列表
    /// </summary>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<BiCustomerField>>>> getPagelist(PageEntity<BiCustomerFieldInput> inputs)
    {
        return Success(await service.getPagelist(inputs));
    }

    /// <summary>
    /// BiCustomerField  单个
    /// </summary>
    [HttpPost]
    [ActionName("getEntity")]
    public async Task<ResponseResult<BiCustomerField>> getEntity(BiCustomerFieldInput input)
    {
        var result = await service.getEntity(input);
        if(result.Item1 == BaseErrorCode.Successful)
        {
            return Success(result.Item2);
        }
        else
        {
            return Error("查询失败", result.Item2);
        }
        
    }

    /// <summary>
    /// BiCustomerField  语法检查
    /// </summary>
    [HttpPost]
    [ActionName("syntaxRules")]
    public async Task<ResponseResult<string>> syntaxRules(BiCustomerFieldInput input)
    {
        var result = await service.syntaxRules(input);
        if (result.Item1.IndexOf("ERROR") != 0)
        {
            return Success(result.Item1, result.Item2);
        }
        else
        {
            return Error(result.Item1, result.Item2);
        }
    }


}

