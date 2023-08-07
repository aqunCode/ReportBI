using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Entity;
using Bi.Services.IService;
using FluentFTP;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Net;

namespace Bi.Services.Service;

public class FtpService : IFtpService {

    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    private ILogger<FtpService> logger;

    public FtpService(ISqlSugarClient _sqlSugarClient
                       , ILogger<FtpService> logger)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.logger = logger;
    }

    public async Task<FtpImageInput> upLoadImage(FtpImageInput input)
    {
        var ftpDirectory = $"/Alan/autoReport";
        IFtpClient ftpClient = null;
        var host = "ftp://10.191.16.30"; //configuration.GetValue<string>("AutoFtpClient:Host");
        var user = "mesadmin";//configuration.GetValue<string>("AutoFtpClient:User");
        var pass = "Luxshare@2022";//configuration.GetValue<string>("AutoFtpClient:Password");
        ftpClient = new FtpClient(host, user, pass);
        if (!(await ftpClient.DirectoryExistsAsync(ftpDirectory)))
            await ftpClient.CreateDirectoryAsync(ftpDirectory);
        int index = input.Data.FileName.LastIndexOf('.');
        var ImageName = $"autoReport_{Guid.NewGuid()}{input.Data.FileName.Substring(index)}";
        var ms = new MemoryStream();
        await input.Data.CopyToAsync(ms);
        //imagePath.Append($@"{ftpDirectory}/{AfterCuringImageName},");
        FtpStatus res = await ftpClient.UploadAsync(ms, ftpDirectory + "/" + ImageName, FtpRemoteExists.Overwrite);
        await ftpClient.DisconnectAsync();
        string ip = getLocalIp();
        return new FtpImageInput
        {
            Url = "http://" + ip + ":8700/api/autoreportcenter/v1/ftpfile/showimage/showimage/" + ImageName
        };
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

    public async Task<MemoryStream> showImage(string imageId)
    {
        string url = "/Alan/autoReport/" + imageId;
        IFtpClient ftpClient = null;
        var host = "ftp://10.191.16.30"; //configuration.GetValue<string>("AutoFtpClient:Host");
        var user = "mesadmin";//configuration.GetValue<string>("AutoFtpClient:User");
        var pass = "Luxshare@2022";//configuration.GetValue<string>("AutoFtpClient:Password");

        ftpClient = new FtpClient(host, user, pass);
        await ftpClient.ConnectAsync();
        var ms = new MemoryStream();
        await ftpClient.DownloadAsync(ms, url);
        byte[] bytes = new byte[ms.Length];
        ms.Read(bytes, 0, bytes.Length);
        ms.Seek(0, SeekOrigin.Begin);
        /*IImageFormat format;
        Image image = Image.Load(ms,out format);*/
        return ms;
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
}

