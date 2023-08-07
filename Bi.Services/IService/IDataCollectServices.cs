using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Entities.Response;
using MagicOnion;
using System.Data;

namespace Bi.Services.IService;

public interface IDataCollectServices :IDependency {
    /// <summary>
    /// 获取数据源列表
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<DataSourceName>> getAllDataSource(DataSourceName input);
    /// <summary>
    /// 添加数据集
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<double> addAsync(DataCollectInput input);
    /// <summary>
    /// 查询所有数据集
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    Task<PageEntity<IEnumerable<DataCollect>>> getEntityListAsync(PageEntity<DataCollectInput> inputs);
    /// <summary>
    /// 查询单个明细数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<DataCollectResponse> getDetailBysetCode(DataCollectInput input);
    /// <summary>
    /// 修改数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<double> ModifyAsync(DataCollectInput input);
    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<double> deleteAsync(DataCollectDelete input);
    /// <summary>
    /// 数据集测试预览
    /// </summary>
    Task<(String, long, long,DataTable)> testTransform(DataCollectInput input);
    /// <summary>
    /// 获取下拉框首值
    /// </summary>
    Task<PageEntity<IEnumerable<object>>> getFirstValues(PageEntity<DataCollectInput> inputs);
    /// <summary>
    /// 获取caseResult中的所有键
    /// </summary>
    public List<string> getSetParamList(string caseResult, string setType, string setDesc);

    /// <summary>
    /// miniExcel 导出之 IDataReader 导出
    /// </summary>
    Task<(IDataReader, string)> testTransform(DataCollectReader input);
    /// <summary>
    /// 获取count的总数
    /// </summary>
    Task<int> testTransform(DataCollectCount dataCollectCount);
}

