using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Json;
/// <summary>
/// 自定义序列化规则
/// </summary>
public class JsonIgnoreAttributeContractResolver : CamelCasePropertyNamesContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        var jsonIgnore = member.GetCustomAttribute<JsonIgnoreAttribute>();
        if (jsonIgnore != null)
        {
            //序列化、反序列化都忽略
            if (jsonIgnore.Serialization == SerializationHandling.SerializeAndDeserialize)
            {
                property.Readable = false;
                property.Writable = false;
            }

            //序列化忽略
            if (jsonIgnore.Serialization == SerializationHandling.SerializeOnly)
            {
                property.Readable = false;
                property.Writable = true;
            }

            //反序列化忽略
            if (jsonIgnore.Serialization == SerializationHandling.DeserializeOnly)
            {
                property.Readable = true;
                property.Writable = false;
            }
        }

        return property;
    }
}
