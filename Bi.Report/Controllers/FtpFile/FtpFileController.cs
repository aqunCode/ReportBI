﻿using Bi.Core.Helpers;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Entity;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Baize.Report.Controllers.FtpFile;

/// <summary>
/// ftp 文件上传
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class FtpFileController : BaseController {

    /// <summary>
    /// ftp 服务接口
    /// </summary>
    private readonly IFtpService services;


    /// <summary>
    /// 构造函数
    /// </summary>
    public FtpFileController(IFtpService services) {
        this.services = services;
    }

    /// <summary>
    /// ftp 图片上传
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("upLoadImage")]
    public async Task<ResponseResult<FtpImageInput>> upLoadImage([FromForm] FtpImageInput input) {
        return Success(await services.upLoadImage(input));
    }

    /// <summary>
    /// ftp 图片显示
    /// </summary>
    /// <param name="imageId"></param>
    /// <returns></returns>
    [HttpGet]
    //[ActionName("showImage/{imageId}")]
    [Route("showImage/{imageId}")]
    public async Task<IActionResult> showImage(string imageId) 
    {

        string path = await services.showImage(imageId);
        var extension = System.IO.Path.GetExtension(imageId);
        string contentType = extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream" // 未知类型，默认为二进制流
        };
        return PhysicalFile(path, contentType, imageId);
    }

    /// <summary>
    /// base64位 图片上传
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult<string>> insertCanvas(Base64ImageInput input)
    {
        input.CurrentUser = CurrentUser;
        return Success(await services.insertCanvas(input));
    }

    /// <summary>
    /// base64位 图片删除
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult<string>> deleteCanvas(Base64ImageInput input)
    {
        input.CurrentUser = CurrentUser;
        return Success(await services.deleteCanvas(input));
    }

    /// <summary>
    /// base64位 图片更新
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("update")]
    public async Task<ResponseResult<string>> updateCanvas(Base64ImageInput input)
    {
        input.CurrentUser = CurrentUser;
        return Success(await services.updateCanvas(input));
    }

    /// <summary>
    /// base64位 图片查询 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getlist")]
    public async Task<ResponseResult<PageEntity<List<Base64ImageEntity>>>> getlist(PageEntity<Base64ImageInput> input)
    {
        input.Data.CurrentUser = CurrentUser;
        return Success(await services.getlist(input));
    }
}

