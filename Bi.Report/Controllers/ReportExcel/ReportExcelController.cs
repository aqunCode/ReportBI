using Bi.Core.Helpers;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Bi.Report.Controllers.ReportExcel;

[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class ReportExcelController : BaseController
{
    /// <summary>
    /// datasource 服务接口
    /// </summary>
    private readonly IReportExcelService service;

    /// <summary>
    /// datasource 构造函数
    /// </summary>
    public ReportExcelController(IReportExcelService service)
    {
        this.service = service;
    }

    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(ReportExcelInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var (res, msg) = await service.AddAsync(new[] { input });
        if (res)
            return Success(msg);

        return Error(msg);
        //if (result > 0)
        //    return Success();
        //else
        //    return Error();
    }

    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modifyAsync(ReportExcelInput input)
    {
        input.CurrentUser = this.CurrentUser;
        var (res, msg) = await service.ModifyAsync(input);
        //if (result > 0)
        //    return Success();
        //else
        //    return Error();
        if (res)
            return Success(msg);

        return Error(msg);
    }

    [HttpGet]
    [ActionName("detailByReportCode")]
    public async Task<ResponseResult<ReportExcelOutput>> detailByReportCode(string ReportCode)
    {
        ReportExcelOutput result = await service.detailByReportCode(ReportCode);
        return Success<ReportExcelOutput>(result);
    }

    [HttpPost]
    [ActionName("firstPreview")]
    public async Task<ResponseResult<ReportExcelOutput>> firstPreview(ReportExcelInput input)
    {
        return Success<ReportExcelOutput>(await service.firstPreview(input));;
    }

    [HttpPost]
    [ActionName("countPreview")]
    public async Task<ResponseResult<JObject>> countPreview(ReportExcelInput input)
    {
        var res = await service.countPreview(input);
        return Success<JObject>(new JObject(new JProperty("total", res.Total),new JProperty("totalPage", res.Total / input.PageSize + (res.Total%input.PageSize == 0? 0:1))));
    }

    [HttpPost]
    [ActionName("preview")]
    public async Task<ResponseResult<ReportExcelOutput>> preview(ReportExcelInput input)
    {
        return Success<ReportExcelOutput>(await service.preview(input));
    }

    [HttpPost]
    [ActionName("export")]
    public async Task<IActionResult> export(ReportExcelInput input)
    {
        input.RequestCount=1;
        input.PageSize= int.MaxValue;
        input.Export = true;
        string contentType;

        var (path, fileName) = await service.exportExcel(input);

        // fileName = "d5741b37-dce6-49b9-be66-4bddb6876201.csv";
        if (System.IO.File.Exists(path+ fileName))
        {
            var extension = System.IO.Path.GetExtension(fileName);
            switch (extension.ToLower())
            {
                case ".zip":
                    contentType = "application/zip";
                    break;
                case ".csv":
                    contentType = "text/csv;charset=utf-8";
                    break;
                case ".xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                default:
                    contentType = fileName.GetContentType();
                    break;
            }
            return PhysicalFile(path+ fileName, contentType,fileName);
        }
        return BadRequest();
    }

    [HttpPost]
    [ActionName("exportUri")]
    public async Task<String> exportUri(ReportExcelInput input)
    {
            

        var (_, fileName) = await service.exportExcel(input);
        return fileName;
    }

    [HttpGet("{fileName}")]
    [ActionName("exportFile")]
    public async Task<IActionResult> exportFile(string fileName)
    {
        // 指定文件的路径
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + $"excel/", fileName);

        // 检查文件是否存在
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        // 使用 FileStreamResult 返回文件
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return new FileStreamResult(fileStream, "application/octet-stream")
        {
            FileDownloadName = fileName
        };
    }
}

