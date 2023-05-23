namespace Bi.Core.Attributes;
/// <summary>
/// 定义一个接口多个实现类时指定注入名称
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ServiceNameAttribute : Attribute
{
    /// <summary>
    /// 注入名称，多继承时唯一标识
    /// </summary>
    public string[] Name { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">注入名称，多继承时唯一标识</param>
    public ServiceNameAttribute(params string[] name)
    {
        Name = name;
    }
}