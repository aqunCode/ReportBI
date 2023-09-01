﻿using Amazon.S3.Model;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IServicep;
using MongoDB.Driver.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

public class ConnectService : IConnectService
{
    /// <summary>
    /// 数据库链接
    /// </summary>
    private SqlSugarScopeProvider repository;

    public ConnectService(ISqlSugarClient _sqlSugarClient)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
    }

    public async Task<TokenResponse> getToken(UserInfo input)
    {
        // 此处验证OA密码，如密码正确自动注册账号

        JwtSettings settings = new ();

        var user = await repository.Queryable<CurrentUser>().FirstAsync(x => x.Account == input.Username && x.Enabled == 1);
        if(user == null)
        {
            return new()
            {
                Access_token = null,
                Refresh_token = null,
                Code = 500
            };
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
