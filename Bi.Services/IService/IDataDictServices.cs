using Bi.Core.Interfaces;
using Bi.Entities.Entity;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IDataDictServices : IDependency {
    /// <summary>
    /// 读取所有数据类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<IEnumerable<DataDict>> getEntityListAsync(DataDictInput input);
}

