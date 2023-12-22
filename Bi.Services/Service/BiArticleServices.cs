using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Core.Const;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bi.Services.Service;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/12/14 10:38:03
/// 版本：1.1
/// </summary>

public class BiArticleServices : IBiArticleServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    private IDbEngineServices dbEngine;

    public BiArticleServices(ISqlSugarClient _sqlSugarClient
                        , IDbEngineServices dbService)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
        this.dbEngine = dbService;
    }



    /// <summary>
    /// 添加新的BiArticle
    /// </summary>
    public async Task<double> addAsync(BiArticleInput input)
    {
        //var inputentitys = await repository.Queryable<BiArticle>().Where(x =>  x.DeleteFlag == "N").ToListAsync();
        //if (inputentitys.Any())
        //    return BaseErrorCode.PleaseDoNotAddAgain;

        var entity = input.MapTo<BiArticle>();
        entity.Create(input.CurrentUser);
        entity.Enabled = input.Enabled;
        await repository.Insertable<BiArticle>(entity).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 根据ID删除BiArticle
    /// </summary>
    public async Task<double> deleteAsync(BiArticleInput input)
    {
        await repository.Deleteable<BiArticle>().In(input.Ids).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 更新BiArticle
    /// </summary>
    public async Task<double> ModifyAsync(BiArticleInput input)
    {
        var set = await repository.Queryable<BiArticle>().FirstAsync(x => x.Id == input.Id);
        input.MapTo<BiArticleInput,BiArticle>(set);
        set.Modify(input.Id,input.CurrentUser);
        await repository.Updateable<BiArticle>(set).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 分页获取所有BiArticle
    /// </summary>
    public async Task<PageEntity<IEnumerable<BiArticle>>> getPagelist(PageEntity<BiArticleInput> inputs)
    {
        //分页查询
        RefAsync<int> total = 0;
        var input = inputs.Data;
        var data = await repository.Queryable<BiArticle>()

            .WhereIF(
                !string.IsNullOrEmpty(input.Id),
                x => x.Id.Contains(input.Id))
            .WhereIF(
                !string.IsNullOrEmpty(input.Title),
                x => x.Title.Contains(input.Title))
            .WhereIF(
                !string.IsNullOrEmpty(input.Content),
                x => x.Content.Contains(input.Content))
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);

        return new PageEntity<IEnumerable<BiArticle>>
        {
            PageIndex = inputs.PageIndex,
            Ascending = inputs.Ascending,
            PageSize = inputs.PageSize,
            OrderField = inputs.OrderField,
            Total = (long)total,
            Data = data
        };
    }

    /// <summary>
    /// 获取所有BiArticle
    /// </summary>
    public async Task<IEnumerable<BiArticle>> getList(BiArticleInput input)
    {
        //分页查询
        var data = await repository.Queryable<BiArticle>()

            .WhereIF(
                !string.IsNullOrEmpty(input.Id),
                x => x.Id.Contains(input.Id))
            .WhereIF(
                !string.IsNullOrEmpty(input.Title),
                x => x.Title.Contains(input.Title))
            .WhereIF(
                !string.IsNullOrEmpty(input.Content),
                x => x.Content.Contains(input.Content))
            .ToListAsync();

        return data;
    }

    public async Task<(double, BiArticle)> getEntity(BiArticleInput input)
    {
        var data = await repository.Queryable<BiArticle>()

            .WhereIF(
                !string.IsNullOrEmpty(input.Id),
                x => x.Id.Contains(input.Id))
            .WhereIF(
                !string.IsNullOrEmpty(input.Title),
                x => x.Title.Contains(input.Title))
            .WhereIF(
                !string.IsNullOrEmpty(input.Content),
                x => x.Content.Contains(input.Content))
            .Take(1)
            .ToListAsync();
        var res = data.FirstOrDefault();
        if (res == null)
            return (BaseErrorCode.Fail,null);
        return (BaseErrorCode.Successful, data.FirstOrDefault());
    }

}

