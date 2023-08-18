using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using SqlSugar;

namespace Bi.Services.Service;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：7/4/2023 3:58:57 PM
/// 版本：1.1
/// </summary>

public class BiCollectServices : IBiCollectServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    private IDbEngineServices dbEngine;

    public BiCollectServices(ISqlSugarClient _sqlSugarClient
                        , IDbEngineServices dbService)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
        this.dbEngine = dbService;
    }
    /// <summary>
    /// 添加新的BiCollect
    /// </summary>
    public async Task<double> addAsync(BiCollectInput input)
    {
        var entity = input.MapTo<BiCollect>();
        entity.Create(input.CurrentUser);
        entity.Enabled = input.Enabled;
        entity.UserId = input.CurrentUser.Account;
        await repository.Insertable<BiCollect>(entity).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 根据ID删除BiCollect
    /// </summary>
    public async Task<double> deleteAsync(BiCollectInput input)
    {
        var set = input.MapTo<BiCollect>();
        await repository.Deleteable<BiCollect>(set).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 获取所有BiCollect
    /// </summary>
    public async Task<IEnumerable<BiCollect>> getList(BiCollectInput input)
    {
        //分页查询
        var data = await repository.Queryable<BiCollect>()
            .Where(x => x.UserId == input.UserId && x.Type == input.Type)
            .ToListAsync();
        return data;
    }

}

