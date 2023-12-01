﻿using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Entities.OutApi;
using Bi.Entities.Response;
using Bi.Services.IOutService;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.OutApi;

/// <summary>
/// 菜单、按钮管理
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class MachineOperateController : BaseController
{
    /// <summary>
    /// 私有字段
    /// </summary>
    private readonly IMachineOperateService MachineOperateService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="menuButtonService"></param>
    public MachineOperateController(IMachineOperateService MachineOperateService)
    {
        this.MachineOperateService = MachineOperateService;
    }

    /// <summary>
    /// 添加操作信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(MachineOperateInput input)
    {
        var result = await MachineOperateService.addAsync(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }

}
