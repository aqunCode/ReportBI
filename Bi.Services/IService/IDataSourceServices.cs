using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using MagicOnion;
using System.Data;
using System.Net;

namespace Bi.Services.IService;

public interface IDataSourceServices :IDependency {
    /// <summary>
    /// 添加数据源
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<double> addAsync(DataSourceInput input);
    /// <summary>
    /// 根据查询条件数据源信息
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    UnaryResult<PageEntity<IEnumerable<DataSource>>> getEntityListAsync(PageEntity<DataSourceInput> inputs);
    /// <summary>
    /// 修改 datasource 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<double> ModifyAsync(DataSourceInput input);
    /// <summary>
    /// 删除 datasource 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<double> deleteAsync(DataSourceDelete input);
    /// <summary>
    /// 测试datasource 连接
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<double> testConnection(DataSourceInput input);
    /// <summary>
    /// 动态DB datasource 查询JSON
    /// </summary>
    UnaryResult<(DataTable, long, long)> testDB(DataCollectDBTest dbTest);
    /// <summary>
    /// miniExcel 导出之 IDataReader 导出
    /// </summary>
    UnaryResult<(IDataReader, string)> testDB(DataReaderDBTest dbTest);
    /// <summary>
    /// 查询sql的总数
    /// </summary>
    UnaryResult<int> testDB(DataCountDBTest dataCountDBTest);
    /// <summary>
    /// 动态http datasource 查询JSON
    /// </summary>
    /// <param name="httpTest"></param>
    /// <returns></returns>
    UnaryResult<(String,HttpStatusCode)> testHttp(DataSourceInput httpTest);
}

