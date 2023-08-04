using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.BIWorkbookData;
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class BIWorkbookDataController : BaseController
{
    /// <summary>
    /// BIWorkbookData 服务接口
    /// </summary>
    private readonly IBIWorkbookDataServices service;

    /// <summary>
    /// BIWorkbookData 构造函数
    /// </summary>
    public BIWorkbookDataController(IBIWorkbookDataServices service)
    {
        this.service = service;
    }


    /// <summary>
    /// 获取 数据集里的表的所有字段
    /// </summary>
    [HttpPost]
    [ActionName("getTableColumn")]
    public async Task<ResponseResult<IEnumerable<ColumnInfo>>> getTableColumn(BIWorkbookInput inputs)
    {
        var value = await service.getTableColumn(inputs);
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
