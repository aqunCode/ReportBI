using System;
using System.Text;
using System.Security.Claims;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Bi.Core.Extensions;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// JwtToken工具类
    /// </summary>
    public class JwtTokenHelper
    {
        /// <summary>
        /// 确定字符串是否是有效格式的Json Web Token (JWT)。 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CanReadToken(string token) =>
            new JwtSecurityTokenHandler().CanReadToken(token);

        /// <summary>
        /// 创建Token
        /// </summary>
        /// <param name="claims">声明集合</param>
        /// <param name="expires">jwt过期时间，例如：DateTime.UtcNow.AddSeconds(7200)</param>
        /// <param name="secret">密钥</param>
        /// <param name="issuer">jwt签发者</param>
        /// <param name="audience">jwt接收方</param>
        /// <param name="notBefore">jwt生效时间，例如：DateTime.UtcNow</param>
        /// <param name="securityAlgorithms">加密类型</param>
        /// <returns></returns>
        public static string CreateToken(
            IEnumerable<Claim> claims,
            DateTime? expires,
            string secret,
            string issuer = null,
            string audience = null,
            DateTime? notBefore = null,
            string securityAlgorithms = SecurityAlgorithms.HmacSha256)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var signingCredentials = new SigningCredentials(key, securityAlgorithms);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: notBefore,
                expires: expires,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        /// <summary>
        /// 解析Token
        /// </summary>
        /// <param name="token">JwtToken字符串</param>
        /// <param name="secret">密钥，不为null时启用token校验</param>
        /// <param name="validationParameters">自定义token校验参数</param>
        /// <returns></returns>
        public static (JwtSecurityToken securityToken, ClaimsPrincipal principal) ReadToken(
            string token,
            string secret = null,
            TokenValidationParameters validationParameters = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadJwtToken(token);

            if (!secret.IsNullOrEmpty())
            {
                //签名
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                //判断是否有自定义TokenValidationParameters
                validationParameters ??= new TokenValidationParameters
                {
                    ValidateIssuer = false,//是否校验issuer
                    ValidateAudience = false,//是否校验audience
                    ValidateLifetime = true,//是否校验失效时间
                    RequireExpirationTime = true,//是否校验expiration
                    ValidateIssuerSigningKey = true,//是否校验securityKey
                    IssuerSigningKey = key//securityKey
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                return (securityToken, principal);
            }

            return (securityToken, null);
        }
    }
}
