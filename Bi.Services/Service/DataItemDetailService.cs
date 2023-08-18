using Amazon.Runtime.Internal.Util;
using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StackExchange.Redis.Role;

namespace Bi.Services.Service;

internal class DataItemDetailService : IDataItemDetailService
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    public DataItemDetailService(ISqlSugarClient _sqlSugarClient)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
    }

    public async Task<IEnumerable<DataItemDetailResponse>> GetListAsync(DataItemDetailQueryInput input)
    {
        //var dt = repository.Ado.GetDataTable("select * from  sys_dataitem");

        var expable = Expressionable.Create<DataItemDetailEntity>();
        //主键Id
        if (!input.Id.IsNullOrEmpty())
            expable = expable.And(x =>x.Id == input.Id);

        //数据字典主表Code
        if (!input.ItemCode.IsNullOrEmpty())
        {
            var dataItemEntity = await repository.Queryable<DataItemEntity>()
                .Where(x => x.Enabled == 1 && x.ItemCode == input.ItemCode).FirstAsync();
            /*var dataItemEntity = repository.FindEntity<DataItemEntity>(x =>
                                        x.Id,
                                        x =>
                                        x.Enabled == 1 &&
                                        x.ItemCode == input.ItemCode);*/
            if (dataItemEntity != null && !dataItemEntity.Id.IsNullOrEmpty())
                expable = expable.And(x => x.ItemId == dataItemEntity.Id);
            else
                return new DataItemDetailResponse[] { };
        }

        //数据字典主表Id
        if (!input.ItemId.IsNullOrEmpty())
            expable = expable.And(x => x.ItemId == input.ItemId);

        //明细编码
        if (!input.DetailCode.IsNullOrEmpty())
            expable = expable.And(x => x.DetailCode == input.DetailCode);

        //明细名称
        if (!input.DetailName.IsNullOrEmpty())
            expable = expable.And(x => input.DetailName.EndsWith(BaseErrorCode.Suffix)
                    ? x.DetailName.Contains(input.DetailName.TrimEnd(BaseErrorCode.Suffix))
                    : x.DetailName == input.DetailName);

        //是否有效
        if (input.Enabled != -1)
            expable = expable.And(x => x.Enabled == input.Enabled);

        var retval = new List<DataItemDetailEntity>();

        if (input.OrderBy.IsNotNullOrEmpty())
            retval = await repository.Queryable<DataItemDetailEntity>().Where(expable.ToExpression()).OrderBy(x=>x.SortCode).ToListAsync();
        else
            retval = await repository.Queryable<DataItemDetailEntity>().Where(expable.ToExpression()).ToListAsync();

        return retval.MapTo<DataItemDetailResponse>();
    }
}
