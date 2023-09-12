using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IServicep;
using Bi.Entities.Response;
using Bi.Core.Const;

namespace Bi.Report.Controllers.UserCenter;

/// <summary>
/// ��ҳ
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class ErrorRecordController : BaseController
{

    /// <summary>
    /// ��ҳ ����ӿ�
    /// </summary>
    private readonly IErrorRecordService errorRecordService;


    /// <summary>
    /// ���캯��
    /// </summary>
    public ErrorRecordController(IErrorRecordService errorRecordService)
    {
        this.errorRecordService = errorRecordService;
    }

    /// <summary>
    /// ��ͷ ���������� ���ʴ��� ������
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("add")]
    public async Task<ResponseResult<string>> insert(ErrorRecordInput input)
    {
        double result = await errorRecordService.insert(input);
        return Success(BaseCode.toChinesCode(result));
    }

}

