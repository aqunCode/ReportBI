using Amazon.Runtime.Internal.Util;
using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using Polly.Caching;
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


    public async Task<double> insertTree(DataItemInput input)
    {
        DataItemEntity menu = input.MapTo<DataItemEntity>();
        menu.Create(input.CurrentUser);
        await repository.Insertable<DataItemEntity>(menu).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> deleteTree(DataItemInput input)
    {
        List<DataItemEntity> list = new();
        foreach (var item in input.multiId)
        {
            list.Add(new DataItemEntity { Id = item });
        }
        await repository.Deleteable<DataItemEntity>(list).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    public async Task<double> modifyTree(DataItemInput input)
    {
        if (string.IsNullOrEmpty(input.Id))
            return BaseErrorCode.Fail;
        DataItemEntity menu = new();
        repository.Tracking(menu);
        input.MapTo<DataItemInput, DataItemEntity>(menu);
        menu.Modify(input.Id, input.CurrentUser);
        await repository.Updateable<DataItemEntity>(menu).ExecuteCommandAsync();
        repository.TempItems.Clear();
        return BaseErrorCode.Successful;
    }

    public async Task<double> insert(DataItemDetailQueryInput input)
    {
        DataItemDetailEntity menu = input.MapTo<DataItemDetailEntity>();
        menu.Create(input.CurrentUser);
        await repository.Insertable<DataItemDetailEntity>(menu).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }
    public async Task<double> delete(DataItemDetailQueryInput input)
    {
        List<DataItemDetailEntity> list = new();
        foreach (var item in input.multiId)
        {
            list.Add(new DataItemDetailEntity { Id = item });
        }
        await repository.Deleteable<DataItemDetailEntity>(list).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }
    public async Task<double> modify(DataItemDetailQueryInput input)
    {
        if (string.IsNullOrEmpty(input.Id))
            return BaseErrorCode.Fail;
        DataItemDetailEntity menu = new();
        repository.Tracking(menu);
        input.MapTo<DataItemDetailQueryInput, DataItemDetailEntity>(menu);
        menu.Modify(input.Id, input.CurrentUser);
        await repository.Updateable<DataItemDetailEntity>(menu).ExecuteCommandAsync();
        repository.TempItems.Clear();
        return BaseErrorCode.Successful;
    }

    public async Task<IEnumerable<DataItemTree>> getDataDictTree()
    {
        List<DataItemTree> tree = new();
        var dicts = await repository.Queryable<DataItemEntity>()
            .OrderBy(x => x.SortCode).ToListAsync();
        dicts.ForEach(x =>
        {
            tree.Add(new DataItemTree
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Title = x.ItemName,
                Code = x.ItemCode,
                SortCode = x.SortCode,
                Expand = true,
                contextmenu = true
            });
        });
        return tree;
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

    public async Task<PageEntity<IEnumerable<DataItemDetailResponse>>> getPagelist(PageEntity<DataItemDetailQueryInput> inputs)
    {
        RefAsync<int> total = 0;
        var list = await repository.Queryable<DataItemDetailEntity>()
            .Where(x => x.ItemId == inputs.Data.ItemId)
            .OrderBy(inputs.OrderField)
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);
        return new PageEntity<IEnumerable<DataItemDetailResponse>>
        {
            PageIndex = inputs.PageIndex,
            Ascending = inputs.Ascending,
            PageSize = inputs.PageSize,
            OrderField = inputs.OrderField,
            Total = total,
            Data = list.MapTo<DataItemDetailResponse>()
        };
    }

}
