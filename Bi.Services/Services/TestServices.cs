using Bi.Services.IServices;
using SqlSugar;
using System.Data;

namespace Bi.Services.Services;

internal class TestServices : ITestServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    public TestServices(ISqlSugarClient _sqlSugarClient)
    {
        this.repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
    }

    public async Task<DataTable> queryAll()
    {
        var result = await repository.Ado.GetDataTableAsync("SELECT * FROM BI_NIFI_STEAM bns OFFSET 0  ROWS FETCH NEXT 1000 ROWS ONLY ");
        return result;
    }
}

