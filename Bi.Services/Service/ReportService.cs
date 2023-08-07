using Bi.Core.Models;
using System.Data;
using Bi.Entities.Input;
using Bi.Services.IService;
using Bi.Entities.Entity;
using MagicOnion;

namespace Bi.Services.Service;

using Bi.Core.Const;
using Bi.Core.Extensions;
using SqlSugar;

public class ReportService : IReportService
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;


    /// <summary>
    /// 构造函数
    /// </summary>
    public ReportService(ISqlSugarClient _sqlSugarClient)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
    }

    //public async UnaryResult<double> AddAsync(ReportInput input)
    //{
    //    if (input.IsNullOrEmpty() || !input.IsValid())
    //        return (false, $"input parameter is invalid");

    //    var inputentity = await repository.FindEntityAsync<Report>(x => x.ReportCode == input.ReportCode);
    //    if (!inputentity.IsNull())
    //        return ErrorCode.PleaseDoNotAddAgain;

    //    var entity = input.MapTo<Report>();
    //    entity.Create(input.CurrentUser);
    //    return await repository.InsertAsync(entity);
    //}

    public async UnaryResult<(bool res, string msg)> AddAsync(IEnumerable<ReportInput> inputs)
    {
        if (inputs.IsNullOrEmpty() || !inputs.IsValid())
            return (false, $"input parameter is invalid");

        foreach(ReportInput input in inputs)
        {
            var reports = await repository.Queryable<AutoReport>().Where(x => x.ReportCode == input.ReportCode  && x.DeleteFlag == 0).ToListAsync() ;
            if (reports.Any())
                return (false, "report code multiplicity");
        }

        var currentUser = inputs.First().CurrentUser;
        var entities = inputs.MapTo<AutoReport>();
        entities.ForEach(entity =>
        {
            entity.Create(currentUser);
        });

        if ((await repository.Insertable(entities).ExecuteCommandAsync()) > 0)
            return (true, "ok");
        return (false, $"fail");
    }

    public async UnaryResult<IEnumerable<AutoReport>> GetEntityListAsync(ReportQueryInput input, bool master = true)
    {
        return await repository.Queryable<AutoReport>()
            .WhereIF(
                input.ReportCode.IsNotNullOrEmpty(),
                x => x.ReportCode == input.ReportCode)
                .WhereIF(
                input.ReportName.IsNotNullOrEmpty(),
                x => x.ReportName.Contains(input.ReportName))
            .WhereIF(
                input.ReportDesc.IsNotNullOrEmpty(),
                x => x.ReportDesc.Contains(input.ReportDesc))
            .WhereIF(
                input.ReportType.IsNotNullOrEmpty(),
                x => x.ReportType == input.ReportType)
            .WhereIF(
                input.ReportAuthor.IsNotNullOrEmpty(),
                x => x.ReportAuthor.Contains(input.ReportAuthor))
            .WhereIF(
                true,
                x => x.DeleteFlag == 0).ToListAsync();
    }

    public async UnaryResult<PageEntity<IEnumerable<AutoReport>>> GetPageListAsync(PageEntity<ReportQueryInput> inputs)
    {
        // 此处数组是权限按钮信息
        ReportQueryInput input = inputs.Data;
        String[] arr = input.CodeList?.Split(',');
        RefAsync<int> total = 0;
        //分页查询
        var data = await repository.Queryable<AutoReport>()
            .WhereIF(
                input.Remark.IsNotNullOrEmpty(),
                x => x.Remark.Contains(input.Remark))
            .WhereIF(
                input.CodeList.IsNotNullOrEmpty(),
                x => arr.Contains(x.ReportCode))
            .WhereIF(
                input.ReportCode.IsNotNullOrEmpty(),
                x => x.ReportCode.Contains(input.ReportCode))
            .WhereIF(
                input.ReportName.IsNotNullOrEmpty(),
                x => x.ReportName.Contains(input.ReportName))
            .WhereIF(
                input.ReportDesc.IsNotNullOrEmpty(),
                x => x.ReportDesc.Contains(input.ReportDesc))
            .WhereIF(
                input.ReportType.IsNotNullOrEmpty(),
                x => x.ReportType == input.ReportType)
            .WhereIF(
                input.ReportAuthor.IsNotNullOrEmpty(),
                x => x.ReportAuthor.Contains(input.ReportAuthor))
            .WhereIF(
                true,
                x => x.DeleteFlag == 0)
            .OrderBy(t => inputs.OrderField, inputs.Ascending ? OrderByType.Asc : OrderByType.Desc)
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);

        if (!String.IsNullOrEmpty(input.SetCode))
        {
            List<AutoReport> res = new List<AutoReport>();
            string[] codeList = data.Select(x => x.ReportCode).ToArray();
            var reportExcels = await repository.Queryable<ReportExcel>().Where(x => x.ReportCode.In(codeList)).ToListAsync();
            foreach(var item in reportExcels)
            {
                if (item.SetCodes.IndexOf(input.SetCode) != -1)
                {
                    res.Add(data.Where(x=>x.ReportCode==item.ReportCode).First());
                }
            }
            data = res;
        }
            
        return new PageEntity<IEnumerable<AutoReport>>
        {
            PageIndex = inputs.PageIndex,
            Ascending = inputs.Ascending,
            PageSize = inputs.PageSize,
            OrderField = inputs.OrderField,
            Total = total,
            Data = data
        };
    }


    public async UnaryResult<(bool res, string msg)> ModifyAsync(ReportInput input)
    {
        if (!input.IsValid())
            return (false, $"input parameter is invalid");

        var reports = await repository.Queryable<AutoReport>().Where(x => x.ReportCode == input.ReportCode && x.DeleteFlag == 0).ToListAsync();
        if (!reports.Any())
        {
            return (false, $"fail");
        }
        var entity = input.MapTo<AutoReport>();
        entity.Modify(reports.First().Id, input.CurrentUser);

        if ((await repository.Updateable(entity).ExecuteCommandAsync()) > 0)
            return (true, "ok");
        return (false, $"fail");
    }

    public async UnaryResult<double> deleteAsync(ReportQueryInput input)
    {
        var reports = await repository.Queryable<AutoReport>().Where(x => x.ReportCode == input.ReportCode).ToListAsync();
        if (!reports.Any())
        {
            return BaseErrorCode.InvalidEncode;
        }
        AutoReport entity = new();
        repository.Tracking(entity);
        entity.Modify(reports.First().Id, input.CurrentUser);
        entity.DeleteFlag = 1;
        return await repository.Updateable(entity).ExecuteCommandAsync();
    }

    public async Task<double> DeleteAsync(string[] ids,CurrentUser currentUser)
    {
        foreach(string id in ids)
        {
            var reports = await repository.Queryable<AutoReport>().Where(a => a.ReportCode == id && a.DeleteFlag == 0).ToListAsync();
            if (reports.Any())
            {
                var deleteEntity = reports.First();
                repository.Tracking(deleteEntity);
                deleteEntity.DeleteFlag = 1;
                deleteEntity.Modify(deleteEntity.Id, currentUser);
                await repository.Updateable(deleteEntity).ExecuteCommandAsync();
            }

        }
        return await Task.FromResult(0);
    }

}

