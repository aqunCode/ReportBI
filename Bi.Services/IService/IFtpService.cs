using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Entity;
using Microsoft.AspNetCore.Http;

namespace Bi.Services.IService;

public interface IFtpService : IDependency {
    /// <summary>
    /// ftp 图片上传
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<FtpImageInput> upLoadImage(FtpImageInput input);
    /// <summary>
    /// ftp 图片url
    /// </summary>
    /// <param name="imageId"></param>
    /// <returns></returns>
    Task<string> showImage(string imageId);
    /// <summary>
    /// 用于前台多个BI报表图片缓存
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> insertCanvas(Base64ImageInput input);
    /// <summary>
    /// 基于ID 和工号删除
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> deleteCanvas(Base64ImageInput input);
    /// <summary>
    /// 基于ID 和工号修改
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> updateCanvas(Base64ImageInput input);
    /// <summary>
    /// 基于工号获取所有图片
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PageEntity<List<Base64ImageEntity>>> getlist(PageEntity<Base64ImageInput> input);
    /// <summary>
    /// 上传用户头像
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> uploadHeadicon(FtpImageInput input);
}

