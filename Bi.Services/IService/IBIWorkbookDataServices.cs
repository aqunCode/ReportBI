using Bi.Core.Interfaces;
using Bi.Entities.Entity;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IBIWorkbookDataServices : IDependency
{
    /// <summary>
    /// 获取数据集表的所有字段
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    Task<(string, IEnumerable<ColumnInfo>)> getTableColumn(BIWorkbookInput inputs);

    Task<(string, IEnumerable<ColumnInfo>)> getColumninfo(string datasetid);
}