using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using System.Data;

namespace Bi.Services.IService;

public interface IBiCalculatorServices : IDependency
{
    /// <summary>
    /// 执行BI设计的查询接口
    /// </summary>
    Task<(string, DataTable)> execute(BIWorkbookInput input);
    /// <summary>
    /// 执行筛选器“加载数据”的功能
    /// </summary>
    Task<(string,PageEntity<List<string>>)> selectValue(PageEntity<ColumnInfo> input);
    /// <summary>
    /// 标记功能获取列所有值
    /// </summary>
    Task<(string, List<string>)> getValueList(MarkValueInput input);

    Task<string> sendWxMessage();
}
