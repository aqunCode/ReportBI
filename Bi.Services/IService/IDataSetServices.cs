using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IDataSetServices: IDependency
{
    /// <summary>
    /// 添加新的数据集
    /// </summary>
    Task<double> addAsync(DataSetInput input);
    /// <summary>
    /// 根据ID删除数据集
    /// </summary>
    Task<double> deleteAsync(DataSetInput input);
    /// <summary>
    /// 更新数据集
    /// </summary>
    Task<double> ModifyAsync(DataSetInput input);
    /// <summary>
    /// 分页获取所有数据集
    /// </summary>
    Task<PageEntity<IEnumerable<BiDataset>>> getPagelist(PageEntity<DataSetInput> inputs);
    /// <summary>
    /// 获取所有的数据集下拉框
    /// </summary>
    Task<IEnumerable<BiDataset>> getSelectlist();
    /// <summary>
    /// 获取当前DB下的的所有用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<(string, IEnumerable<string>)> getUserlist(TableInput input);
    /// <summary>
    /// 获取当前DB用户下的所有表
    /// </summary>
    /// <returns></returns>
    Task<(string,IEnumerable<string>)> getTablelist(TableInput input);
    /// <summary>
    /// 获取当前DB用户下的表的所有字段
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<(string, IEnumerable<ColumnInfo>)> getColumnlist(TableInput input);
}

