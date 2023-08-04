using Bi.Core.Const;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.AutoReport.DataSetRelation;

[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class DataSetRelationController : BaseController
{

    /// <summary>
    /// DataCollect 服务接口
    /// </summary>
    private readonly IDataSetRelationServices service;

    /// <summary>
    /// DataCollect 构造函数
    /// </summary>
    public DataSetRelationController(IDataSetRelationServices service)
    {
        this.service = service;
    }

    /// <summary>
    /// DataCollect 添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(DataSetRelationInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var result = await service.addAsync(input);
        if (result == BaseErrorCode.Successful)
            return Success(result.ToString());
        else
            return Error(result.ToString());
    }
}
