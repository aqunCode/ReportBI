using Bi.Core.Extensions;
using Bi.Core.Helpers;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Response;
using Bi.Services.IServicep;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

internal class ConnectService : IConnectService
{
    public async Task<TokenResponse> getToken(UserInfo input)
    {
        JwtSettings settings = new ();
        // 假装查询了数据库
        CurrentUser user = new CurrentUser
        {
            Account = input.Username,
            Id = "sdfghjkjhgfdsdfghjkkkkkkkksdfs",
            Name = "葛鹏飞",
            Email = "1.qq.com",
            SystemFlag = "Y",
            RoleIds = "a,b,c,d,e",
            CompanyIds = "hostar",
            DepartmentIds = "it",
            HeadIcon = "coin",
            Source = 1
        };


        List<Claim> claims = new();
        claims.Add(new Claim(UserClaimTypes.Account, user.Account));
        claims.Add(new Claim(UserClaimTypes.UserId, user.Id));
        claims.Add(new Claim(ClaimTypes.Name, user.Name));
        claims.Add(new Claim(ClaimTypes.Email, user.Email));
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

        TokenResponse response = new();
        response.Access_token = token;
        response.Refresh_token = token;
        response.Code = 200;
        response.Result = new CurrentUserResponse
        {
            NeedChangePassword = false
        };
        return response;
    }
}
