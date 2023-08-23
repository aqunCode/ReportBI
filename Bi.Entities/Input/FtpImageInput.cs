using Bi.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Bi.Entities.Input;

public class FtpImageInput : BaseInput {
    /// <summary>
    /// 图片url图片地址
    /// </summary>
    public string? Url { get; set; }
    /// <summary>
    /// 图片form提交
    /// </summary>
    public IFormFile? Data { get; set; }
    /// <summary>
    /// 头像form提交
    /// </summary>
    public IFormFile? File { get; set; }
    /// <summary>
    /// 图片二进制
    /// </summary>
    public byte[]? ImageByte { get; set; }
}

public class Base64ImageInput : BaseInput
{
    public string? ImageJson { get; set; }

    public string? Id { get; set; }
}


