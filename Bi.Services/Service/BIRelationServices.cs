using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

internal class BIRelationServices : IBIRelationServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    public BIRelationServices(ISqlSugarClient _sqlSugarClient
                        , IDbEngineServices dbService)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
    }



    /// <summary>
    /// 添加新的数据集
    /// </summary>
    public async Task<double> addAsync(BIRelationInput input)
    {
        var inputentitys = await repository.Queryable<BIRelation>().Where(x => x.SourceId == input.DatasetId && x.TargetId == input.FatherId && x.DeleteFlag == "N").ToListAsync();
        if (inputentitys.Any())
            return BaseErrorCode.PleaseDoNotAddAgain;

        var entity = input.MapTo<BIRelation>();
        entity.Create(input.CurrentUser);
        return await repository.Insertable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据ID删除数据集
    /// </summary>
    public async Task<double> deleteAsync(BIRelationInput input)
    {
        var set = input.MapTo<BIRelation>();
        repository.Tracking(set);
        set.DeleteFlag = "Y";
        var res = await repository.Updateable<BIRelation>(set).ExecuteCommandAsync();
        if (res <= 0)
            return BaseErrorCode.Fail;
        else
            return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 更新数据集
    /// </summary>
    public async Task<double> ModifyAsync(BIRelationInput input)
    {
        BIRelation relation = new();
        repository.Tracking(relation);
        input.MapTo<BIRelationInput,BIRelation>(relation);
        relation.Modify(input.Id,input.CurrentUser);
        var res = await repository.Updateable<BIRelation>(relation).Where(x => x.DeleteFlag == "N").ExecuteCommandAsync();
        if (res <= 0)
            return BaseErrorCode.Fail;
        else
            return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 分页获取所有数据集
    /// </summary>
    public async Task<PageEntity<IEnumerable<BIRelation>>> getPagelist(PageEntity<BIRelationInput> inputs)
    {
        //分页查询
        RefAsync<int> total = 0;
        var input = inputs.Data;
        var data = await repository.Queryable<BIRelation>()
            .WhereIF(
                !input.Id.IsNullOrEmpty(),
                x => x.Id == input.Id)
            .WhereIF(
                !input.DatasetCode.IsNullOrEmpty(),
                x => x.DatasetCode == input.DatasetCode)
            .WhereIF(
                !input.DatasetId.IsNullOrEmpty() && !input.FatherId.IsNullOrEmpty(),
                x => x.SourceId == input.DatasetId && input.FatherId == x.TargetId)
            .WhereIF(
                true,
                x => x.DeleteFlag == "N")
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);

        return new PageEntity<IEnumerable<BIRelation>>
        {
            PageIndex = inputs.PageIndex,
            Ascending = inputs.Ascending,
            PageSize = inputs.PageSize,
            OrderField = inputs.OrderField,
            Total = (long)total,
            Data = data
        };
    }
}
