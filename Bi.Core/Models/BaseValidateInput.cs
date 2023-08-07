using Bi.Core.Helpers;
using Bi.Core.Json;
using MessagePack;

namespace Bi.Core.Models
{
    /// <summary>
    /// 基本验证
    /// </summary>
    [MessagePackObject(true)]
    public abstract class BaseValidateInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonIgnore(SerializationHandling.SerializeOnly)]
        public string AppId { get; set; }

        /// <summary>
        /// key
        /// </summary>
        [JsonIgnore(SerializationHandling.SerializeOnly)]
        public string AppKey { get; set; }

        /// <summary>
        /// 扩展字段
        /// </summary>
        [JsonIgnore(SerializationHandling.SerializeOnly)]
        public string Extend { get; set; }

        /// <summary>
        /// 校验AppId和AppKey是否有效
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public bool Validate(string appid = null, string appkey = null)
        {
            appid ??= ConfigHelper.Get<string>("OpenApi:AppId");
            appkey ??= ConfigHelper.Get<string>("OpenApi:AppKey");

            return this.AppId == appid && this.AppKey == appkey;
        }
    }
}
