using Bi.Core.Caches;
using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bi.Core.Models
{
    /// <summary>
    /// 当前操作者
    /// </summary>
    public class Operator
    {
        /// <summary>
        /// 根据Token获取当前操作者信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static CurrentUser GetUserFromToken(string token)
        {
            if (token.IsNotNullOrEmpty())
            {
                token = token.Replace("Bearer ", "");
                var (securityToken, principal) = JwtTokenHelper.ReadToken(token);
                var claims = securityToken.Claims;

                var account =  claims.FirstOrDefault(x => x.Type == UserClaimTypes.Account)?.Value;

                var user = new CurrentUser
                {
                    //赋值用户信息
                    Password = "***",
                    Account = account,
                    Id = claims.FirstOrDefault(x => x.Type == UserClaimTypes.UserId)?.Value,
                    Name = claims.FirstOrDefault(x => x.Type == "Name")?.Value,
                    Email = claims.FirstOrDefault(x => x.Type == "Email")?.Value,
                    SystemFlag = claims.FirstOrDefault(x => x.Type == UserClaimTypes.SystemFlag)?.Value?.ToLower(),
                    RoleIds = claims.FirstOrDefault(x => x.Type == UserClaimTypes.RoleId)?.Value,
                    CompanyIds = claims.FirstOrDefault(x => x.Type == UserClaimTypes.CompanyId)?.Value,
                    DepartmentIds = claims.FirstOrDefault(x => x.Type == UserClaimTypes.DepartmentId)?.Value,
                    HeadIcon = claims.FirstOrDefault(x => x.Type == UserClaimTypes.HeadIcon)?.Value,
                    Source = int.Parse(claims.FirstOrDefault(x => x.Type == UserClaimTypes.Source)?.Value ?? "0"),
                    Enabled = 1
                };

                user.IsAdministrator = AppSettings.IsAdministrator(user.Account);

                return user;
            }

            return null;
        }

        /// <summary>
        /// 校验token是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static async Task<bool> ValidateTokenAsync(string token, ICache cache)
        {
            if (token.IsNullOrEmpty())
                return false;

            if (!JwtTokenHelper.CanReadToken(token))
                return false;

            var (securityToken, _) = JwtTokenHelper.ReadToken(token);
            if (securityToken.IsNull() || securityToken.Claims.IsNullOrEmpty())
                return false;

            var account = securityToken.Claims.FirstOrDefault(x => x.Type == UserClaimTypes.Account)?.Value;
            var source = securityToken.Claims.FirstOrDefault(x => x.Type == UserClaimTypes.Source)?.Value;

            if (account.IsNull() || source.IsNull())
                return false;

            var cacheToken = await cache.GetAsync<JwtToken>($"{account}__{source}");
            if (cacheToken == null)
                return false;

            //判断是否为最新token
            if (cacheToken.AccessToken != token)
                return false;

            if (cacheToken.AccessTokenExpire < DateTime.Now)
                return false;

            return true;
        }
    }
}
