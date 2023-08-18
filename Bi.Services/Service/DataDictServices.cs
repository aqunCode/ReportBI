using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using SqlSugar;

namespace Bi.Services.Service;

public class DataDictServices : IDataDictServices {
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    public DataDictServices(ISqlSugarClient _sqlSugarClient) {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
    }

    public async Task<IEnumerable<DataDict>> getEntityListAsync(DataDictInput input) {
        var list = await repository.Queryable<DataDict>().Where(x => x.DeleteFlag == 0 && x.Enabled == 1 && x.ChartType == input.ChartType).ToListAsync();
        return list;
    }
}

