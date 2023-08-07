using Bi.Core.Interfaces;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IBIMarkServices : IDependency
{
    /// <summary>
    /// 分析mark标记来生成sql
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<(string, string)> AnalysisMarkSql(BIWorkbookInput input);
    Task<(string, List<string>)> getValueList(MarkValueInput input);
}
