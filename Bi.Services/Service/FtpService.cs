using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Entity;
using Bi.Services.IService;
using FluentFTP;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Net;
using SixLabors.ImageSharp;
using System.IO;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Tga;
using Bi.Core.Const;

namespace Bi.Services.Service;

public class FtpService : IFtpService
{

    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    private ILogger<FtpService> logger;


    public FtpService(ISqlSugarClient _sqlSugarClient
                       , ILogger<FtpService> logger)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
        this.logger = logger;
    }

    public async Task<FtpImageInput> upLoadImage(FtpImageInput input)
    {
        var imageFile = input.Data;
        if (imageFile == null)
        {
            throw new Exception("请选择要上传的图片！");
        }

        var imageName = $"autoReport_{Path.GetRandomFileName()}{Path.GetExtension(imageFile.FileName)}";
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        var imagePath = Path.Combine(filePath, imageName);

        using (var stream = imageFile.OpenReadStream())
        using (var image = Image.Load(stream))
        {
            image.Mutate(x => x.BackgroundColor(Color.Transparent)); // 设置背景色为透明

            //var encoder = new PngEncoder();
            var encoder = getEncoder(Path.GetExtension(imageFile.FileName));
            image.Save(imagePath, encoder);
        }
        string ip = "localhost";
        // string ip = getLocalIp();
        return new FtpImageInput
        {
            Url = "http://" + ip + ":8700/ftpfile/showimage/showimage/" + imageName
        };
    }

    private IImageEncoder getEncoder(string extension)
    {
        switch (extension.ToLower())
        {
            case ".tga":
                return new TgaEncoder();
            case ".bmp":
                return new BmpEncoder();
            case ".png":
                return new PngEncoder();
            case ".jpg":
            case ".jpeg":
                return new JpegEncoder();
            case ".gif":
                return new GifEncoder();
            default:
                return new PngEncoder();
        }
    }

    private string getLocalIp()
    {
        string hostName = Dns.GetHostName();
        IPHostEntry iPHostEntry = Dns.GetHostEntry(hostName);
        var addressV = iPHostEntry.AddressList.FirstOrDefault(q => q.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);//ip4地址
        if (addressV != null)
            return addressV.ToString();
        return "localhost";
    }

    public async Task<string> showImage(string imageId)
    {
        string ip = getLocalIp();
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image", imageId);
        return filePath;
    }

    public async Task<string> insertCanvas(Base64ImageInput input)
    {
        var exits = await repository.Queryable<Base64ImageEntity>().Where(x => x.CreateUserId == input.CurrentUser.Account && x.Id == input.Id).ToListAsync();
        if (exits.Any())
        {
            return await updateCanvas(input);
        }
        var entity = input.MapTo<Base64ImageEntity>();
        entity.Create(input.CurrentUser, input.Id);
        await repository.Insertable<Base64ImageEntity>(entity).ExecuteCommandAsync();
        logger.LogInformation($" {input.CurrentUser.Account} : {input.Id}");
        // 删除超过24小时之前的数据
        await repository.Deleteable<Base64ImageEntity>().Where(x => x.CreateDate < DateTime.Now.AddDays(-1)).ExecuteCommandAsync();
        return "OK";
    }

    public async Task<string> deleteCanvas(Base64ImageInput input)
    {
        var res = await repository.Deleteable<Base64ImageEntity>().Where(x => x.Id == input.Id && x.CreateUserId == input.CurrentUser.Account).ExecuteCommandAsync();
        logger.LogInformation($" {input.CurrentUser.Account} : {input.Id}");
        return $"OK delete {res} rows";
    }

    public async Task<string> updateCanvas(Base64ImageInput input)
    {
        Base64ImageEntity entity = await repository.Queryable<Base64ImageEntity>()
                                .Where(x => x.CreateUserId == input.CurrentUser.Account && x.Id == input.Id).FirstAsync();
        entity.ImageJson = input.ImageJson;
        entity.Modify(input.Id, input.CurrentUser);
        var res = await repository.Updateable(entity).WhereColumns(it => new { it.Id, it.CreateUserId }).ExecuteCommandAsync();
        logger.LogInformation($" {input.CurrentUser.Account} : {input.Id}");
        return $"OK update {res} rows";
    }

    public async Task<PageEntity<List<Base64ImageEntity>>> getlist(PageEntity<Base64ImageInput> input)
    {
        string[] arr = input.Data.Id?.Split(',');
        RefAsync<int> total = 0;
        var result = await repository.Queryable<Base64ImageEntity>()
            .Where(x => x.CreateUserId == input.Data.CurrentUser.Account)
            .WhereIF(
                arr != null,
                x => arr.Contains(x.Id)
            )
            .ToPageListAsync(input.PageIndex, input.PageSize, total);

        return new PageEntity<List<Base64ImageEntity>>
        {
            Ascending = false,
            Total = total,
            PageSize = input.PageSize,
            PageIndex = input.PageIndex,
            Data = result
        };
    }

    public async Task<string> uploadHeadicon(FtpImageInput input)
    {
        var imageFile = input.File;
        if (imageFile == null)
        {
            throw new Exception("请选择要上传的图片！");
        }

        var imageName = $"coin_{Path.GetRandomFileName()}{Path.GetExtension(imageFile.FileName)}";
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "picture");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        var imagePath = Path.Combine(filePath, imageName);

        using (var stream = imageFile.OpenReadStream())
        using (var image = Image.Load(stream))
        {
            image.Mutate(x => x.BackgroundColor(Color.Transparent)); // 设置背景色为透明

            //var encoder = new PngEncoder();
            var encoder = getEncoder(Path.GetExtension(imageFile.FileName));
            image.Save(imagePath, encoder);
        }
        #region 修改账户信息（注释）
        /*string account = input.CurrentUser.Account;
        if (account.IsNullOrEmpty())
            throw new Exception("账号异常！");


        var user = await repository.Queryable<CurrentUser>().FirstAsync(x => x.Account == account);
        if(user == null)
            throw new Exception("请选择要上传的图片！");

        user.HeadIcon = imageName;
        await repository.Updateable< CurrentUser >(user).ExecuteCommandAsync();*/
        #endregion

        return imageName;
    }
}

