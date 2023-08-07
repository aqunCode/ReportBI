using Bi.Core.Interfaces;
using Bi.Entities.Entity;

namespace Bi.Services.IService;

public interface ISyntaxServices :IDependency
{
    /// <summary>
    /// 解析自定义语法
    /// </summary>
    /// <param name="fieldFunction">自定义字段的自定义语法</param>
    /// <param name="sourceType">数据源类型</param>
    /// <param name="fieldCode">自定义字段的名称</param>
    /// <param name="dic">列名对应的数据类型</param>
    /// <returns>返回解析完之后的sql</returns>
    (string,string) syntaxFuction(string fieldFunction, string sourceType,string fieldCode,Dictionary<string, SyntaxDataType> dic);
}
