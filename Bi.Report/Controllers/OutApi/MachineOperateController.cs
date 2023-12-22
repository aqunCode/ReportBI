using Bi.Core.Models;
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
    /// 员工上线操作
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> onLine(MachineOperateInput input)
    {
        var result = await MachineOperateService.addAsync(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }

    /*/// <summary>
    /// 员工上线操作
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("offLine")]
    public async Task<ResponseResult> offLine(MachineOperateInput input)
    {
        var result = await MachineOperateService.updateAsync(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }*/

    /// <summary>
    /// 获取员工岗位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPost")]
    public async Task<ResponseResult> getPost(MachineOperateInput input)
    {
        return Success(await MachineOperateService.getPost(input));
    }
}

