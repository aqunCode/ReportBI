using System;

namespace Bi.Core.Models
{
    /// <summary>
    /// JwtToken相应对象
    /// iss (issuer)：签发人
    /// exp (expiration time)：过期时间
    /// sub (subject)：主题
    /// aud (audience)：受众
    /// nbf (Not Before)：生效时间
    /// iat (Issued At)：签发时间
    /// jti (JWT ID)：编号
    /// </summary>
    public class JwtToken
    {
        /// <summary>
        /// jwt访问token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 刷新token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// jwt访问token过期时间
        /// </summary>
        public DateTime AccessTokenExpire { get; set; }

        /// <summary>
        /// 刷新token过期时间
        /// </summary>
        public DateTime RefreshTokenExpire { get; set; }

        /// <summary>
        /// 服务器当前时间
        /// </summary>
        public DateTime ServerTime { get; set; } = DateTime.Now;
    }
}
