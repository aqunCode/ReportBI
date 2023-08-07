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
    /// 获取数据字典明细列表
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getlist")]
    public async Task<ResponseResult<IEnumerable<DataItemDetailResponse>>> GetListAsync(DataItemDetailQueryInput input)
    {
        var data = await _dataItemDetailService.GetListAsync(input, false);
        return Success(data);
    }

}