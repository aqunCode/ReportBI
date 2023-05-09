namespace Bi.Core.Json;
/// <summary>
/// Json序列化、反序列化忽略特性
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class JsonIgnoreAttribute : Attribute
{
    public readonly SerializationHandling Serialization;

    public JsonIgnoreAttribute(SerializationHandling serialization = SerializationHandling.SerializeAndDeserialize)
    {
        Serialization = serialization;
    }
}

/// <summary>
/// 序列化处理策略
/// </summary>
public enum SerializationHandling
{
    /// <summary>
    /// 序列化、反序列化都忽略
    /// </summary>
    SerializeAndDeserialize = 0,

    /// <summary>
    /// 仅在序列化时忽略
    /// </summary>
    SerializeOnly = 1,

    /// <summary>
    /// 仅在反序列化时忽略
    /// </summary>
    DeserializeOnly = 2,
}