using Amazon.S3.Model;
using Azure.Core;
using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IServicep;
using org.apache.zookeeper.data;
using SqlSugar;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static org.apache.zookeeper.KeeperException;

namespace Bi.Services.Service;

public class ConnectService : IConnectService
{
    /// <summary>
    /// 数据库链接
    /// </summary>
    private SqlSugarScope scope;

    public ConnectService(ISqlSugarClient _sqlSugarClient)
    {
        scope = _sqlSugarClient as SqlSugarScope;
    }

    public async Task<TokenResponse> getToken(UserInfo input)
    {
        JwtSettings settings = new();
        var repository =  scope.GetConnectionScope("oadb");
        var flag = AppSettings.IsAdministrator(input.Username) == 1;
        // 此处验证OA密码，如密码正确自动注册账号
        string password = input.Password.ToMd5();
        
        var oaUser = await repository.SqlQueryable<CurrentUser>($@"select a.loginid account,
                                                                    a.password ,
                                                                    a.lastname name,
                                                                    'coin.png' headIcon ,
                                                                    a.pinyinlastname simpleSpelling,
                                                                    a.mobile phone,
                                                                    c.departmentname  departmentIds,
                                                                    b.subcompanyname  companyIds,
                                                                    REPLACE(a.pinyinlastname,'^','' ) + '@Hostar.com' email,
                                                                    'bi' systemFlag,
                                                                    1 source
                                                                    from HrmPinYinResource a 
                                                                    left join HrmSubCompany b on a.subcompanyid1 = b.id 
                                                                    left join HrmDepartment c on a.departmentid = c.id 
                                                                    where loginid ='{input.Username}'").FirstAsync();

        if (!flag &&( oaUser == null || password != oaUser.Password))
        {
            return new()
            {
                Access_token = null,
                Refresh_token = null,
                Code = BaseErrorCode.ErrorDetail,
                Message = "用户名或密码错误"
            };
        }

        repository = scope.GetConnectionScope("bidb");

        var user = await repository.Queryable<CurrentUser>().FirstAsync(x => x.Account == input.Username && x.Enabled == 1);

        if(flag && password != user.Password)
        {
            return new()
            {
                Access_token = null,
                Refresh_token = null,
                Code = BaseErrorCode.ErrorDetail,
                Message = "用户名或密码错误"
            };
        }

        if(user == null)
        {
            // 获取默认用户权限
            var initUser = await repository.Queryable<CurrentUser>().Where(x => x.Account == "init_user").FirstAsync();

            // 创建新用户
            oaUser.Id = Sys.Guid;
            oaUser.CreateDate = DateTimeExtensions.Now();
            oaUser.CreateUserId = "sytuser";
            oaUser.CreateUserName = "sysuser";
            oaUser.Enabled = 1;
            oaUser.RoleIds = initUser.RoleIds;
            oaUser.LastPasswordChangeTime = DateTimeExtensions.Now();
            var code = await repository.Insertable<CurrentUser>(oaUser).ExecuteCommandAsync();
            if(code == 0)
                return new()
                {
                    Access_token = null,
                    Refresh_token = null,
                    Code = BaseErrorCode.ErrorDetail,
                    Message = "oa账户同步失败"
                };
            user = oaUser;
        }
        List<Claim> claims = new();
        claims.Add(new Claim(UserClaimTypes.Account, user.Account));
        claims.Add(new Claim(UserClaimTypes.UserId, user.Id));
        claims.Add(new Claim("Name", user.Name));
        claims.Add(new Claim("Email", user.Email));
        claims.Add(new Claim(UserClaimTypes.SystemFlag, user.SystemFlag));
        claims.Add(new Claim(UserClaimTypes.RoleId, user.RoleIds));
        claims.Add(new Claim(UserClaimTypes.CompanyId, user.CompanyIds));
        claims.Add(new Claim(UserClaimTypes.DepartmentId, user.DepartmentIds));
        claims.Add(new Claim(UserClaimTypes.HeadIcon, user.HeadIcon));

        var token = JwtTokenHelper.CreateToken(claims, 
                                   DateTime.UtcNow.AddSeconds(settings.AccessTokenExpire),
                                   settings.Secret,
                                   settings.Issuer,
                                   settings.Audience,
                                   DateTime.UtcNow);

        return new()
        {
            Access_token = token,
            Refresh_token = token,
            Code = 200,
            Result = new CurrentUserResponse
            {
                NeedChangePassword = false
            }
        };
    }
}
