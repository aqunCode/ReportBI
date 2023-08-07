namespace Bi.Core.Models
{
    /// <summary>
    /// Jwt配置
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// 签名密钥
        /// </summary>
        public string Secret { get; set; } = "zqK7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols=";

        /// <summary>
        /// 颁发者
        /// </summary>
        public string Issuer { get; set; } = "bi";

        /// <summary>
        /// 受众
        /// </summary>
        public string Audience { get; set; } = "bi";

        /// <summary>
        /// jwt生效时间(nbf)与当前utc时间偏移，单位(s)
        /// </summary>
        public int NotBeforeSkew { get; set; } = 0;

        /// <summary>
        /// jwt访问token过期时间，172800 (48h)，单位(s)
        /// </summary>
        public int AccessTokenExpire { get; set; } = 172800;

        /// <summary>
        /// 刷新token过期时间，2592000(30d)，单位(s)
        /// </summary>
        public int RefreshTokenExpire { get; set; } = 2592000;

        /// <summary>
        /// 是否启用api接口授权校验，默认不启用
        /// </summary>
        public bool ApiAuthorize { get; set; } = false;
    }
}
