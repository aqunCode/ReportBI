using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System.Data;

namespace Bi.Services.Service;

public class BIWorkbookServices : IBIWorkbookServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;
    /// <summary>
    /// 行列服务接口
    /// </summary>
    private readonly IAggregationServices aggregationServices;
    /// <summary>
    /// relation 服务接口
    /// </summary>
    private readonly IBinaryTreeServices rService;
    /// <summary>
    /// 筛选器服务接口
    /// </summary>
    private readonly IBIFilterServices filterServices;
    /// <summary>
    /// sql处理引擎
    /// </summary>
    private IDbEngineServices dbEngineService;
    /// <summary>
    /// 标记服务接口
    /// </summary>
    private readonly IBIMarkServices markServices;

    public BIWorkbookServices(ISqlSugarClient _sqlSugarClient
                                , IBinaryTreeServices service
                                , IBIFilterServices filterServices
                                , IAggregationServices aggregationServices
                                , IDbEngineServices dbService
                                , IBIMarkServices markServices)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.rService = service;
        this.filterServices = filterServices;
        this.aggregationServices = aggregationServices;
        this.dbEngineService = dbService;
        this.markServices = markServices;
    }
    /// <summary>
    /// 添加新的工作簿
    /// </summary>
    public async Task<string> addAsync(BIWorkbookInput input)
    {
        input.MarkItems = input.MarkItems ?? new List<BiMarkField>();
        var inputentitys = await repository.Queryable<BIWorkbook>().Where(x => x.WorkBookCode == input.WorkBookCode && x.DeleteFlag == "N").ToListAsync();
        if (inputentitys.Any())
            return "请勿重复插入！";

        var entity = input.MapTo<BIWorkbook>();

        
        // 生成主键ID
        entity.Create(input.CurrentUser);

        foreach(var item in input.CalcItems)
        {
            item.Create(input.CurrentUser);
            item.WorkbookId = entity.Id;
            item.SortValueJson = item.SortValue == null ? null : JsonConvert.SerializeObject(item.SortValue);
        }
        for (int i = 0; i < input.FilterItems.Count; i++)
        {
            input.FilterItems[i].Create(input.CurrentUser);
            input.FilterItems[i].WorkbookId = entity.Id;
            input.FilterItems[i].OrderBy = i;
        }
        if (input.MarkItems != null)
        {
            foreach (var item in input.MarkItems)
            {
                item.Create(input.CurrentUser);
                item.WorkbookId = entity.Id;
                item.SortValueJson = item.SortValue == null ? null : JsonConvert.SerializeObject(item.SortValue);
            }
        }

        // 开启事务执行插入
        try
        {
            repository.Ado.BeginTran();
            await repository.Insertable<BIWorkbook>(entity).ExecuteCommandAsync();
            await repository.Insertable<BiCalcField>(input.CalcItems).ExecuteCommandAsync();
            await repository.Insertable<BiFilterField>(input.FilterItems).ExecuteCommandAsync();
            await repository.Insertable<BiMarkField>(input.MarkItems).ExecuteCommandAsync();
            repository.Ado.CommitTran();
        }
        catch (Exception ex)
        {
            repository.Ado.RollbackTran();
            return ex.Message;
        }
        return "OK";
    }

    /// <summary>
    /// 删除工作簿
    /// </summary>
    public async Task<string> deleteAsync(BIWorkbookInput input)
    {
        var set = await repository.Queryable<BIWorkbook>().FirstAsync(x => x.Id == input.Id);
        if(set == null)
        {
            return "ERROR 工作簿ID不存在";
        }
        set.Modify(input.Id,input.CurrentUser);
        set.DeleteFlag = "Y";

        var deleteCalists = await repository.Queryable<BiCalcField>()
               .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId.Contains(input.Id)).ToListAsync();
        var deleteFilists = await repository.Queryable<BiFilterField>()
               .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId.Contains(input.Id)).ToListAsync();
        var deleteMarklists = await repository.Queryable<BiMarkField>()
               .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId.Contains(input.Id)).ToListAsync();

        foreach (var item in deleteCalists)
        {
            item.DeleteFlag = "Y";
            item.Modify(item.Id,input.CurrentUser);
        }
        foreach (var item in deleteFilists)
        {
            item.DeleteFlag = "Y";
            item.Modify(item.Id, input.CurrentUser);
        }
        foreach (var item in deleteMarklists)
        {
            item.DeleteFlag = "Y";
            item.Modify(item.Id, input.CurrentUser);
        }

        try
        {
            repository.Ado.BeginTran();
            await repository.Updateable<BIWorkbook>(set).ExecuteCommandAsync();
            //根据WorkbookId删除对应的计算字段
            await repository.Updateable<BiCalcField>(deleteCalists).ExecuteCommandAsync();
            //根据WorkbookId删除对应的筛选字段 
            await repository.Updateable<BiFilterField>(deleteFilists).ExecuteCommandAsync();
            //根据WorkbookId删除对应的标记字段
            await repository.Updateable<BiMarkField>(deleteMarklists).ExecuteCommandAsync();
            repository.Ado.CommitTran();
        }
         catch (Exception ex)
        {
            repository.Ado.RollbackTran();
            return ex.Message;
        }
        return "OK";
    }
    /// <summary>
    /// 编辑工作簿和预览工作簿
    /// </summary>
    public async Task<(string,BIWorkbookInput)> getEchoAsync(BIWorkbookInput input)
    {
        var workbook = await repository.Queryable<BIWorkbook>()
            .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.Id.Contains(input.Id))
            .WhereIF(
                true,
                x => x.DeleteFlag == "N")
            .WhereIF(input.NodeId.IsNotNullOrEmpty(), t => t.Opt2 == input.NodeId)
            .WhereIF(input.WorkBookName.IsNotNullOrEmpty(), t => t.WorkBookName == input.WorkBookName)
            .WhereIF(input.WorkBookCode.IsNotNullOrEmpty(), t => t.WorkBookCode == input.WorkBookCode)
            .ToListAsync();

        var cal = await repository.Queryable<BiCalcField>()
            .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId.Contains(input.Id)).ToListAsync();
        var Fil = await repository.Queryable<BiFilterField>()
            .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId.Contains(input.Id)).ToListAsync();
        var Mark = await repository.Queryable<BiMarkField>()
            .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId.Contains(input.Id)).ToListAsync();
        cal = cal.OrderBy(x => x.Axis).ThenBy(x => x.OrderBy).ToList();
        Fil = Fil.OrderBy(x => x.OrderBy).ToList();
        var bi = workbook.First();    
        bi.FilterItems = Fil;        
        bi.CalcItems = cal;
        bi.MarkItems = Mark;

        foreach(var item in cal)
        {
            item.SortValue = item.SortValueJson == null ? null: JsonConvert.DeserializeObject<List<SortField>>(item.SortValueJson);
        }
        foreach (var item in Mark)
        {
            item.SortValue = item.SortValueJson == null ? null : JsonConvert.DeserializeObject<List<SortField>>(item.SortValueJson);
        }

        return ("OK",bi.MapTo<BIWorkbookInput>());
    }
    /// <summary>
    /// 查询工作簿
    /// </summary>
    public async Task<PageEntity<IEnumerable<BIWorkbook>>> getEntityListAsync(PageEntity<BIWorkbookInput> inputs)
    {
        //分页查询
        RefAsync<int> total = 0;
        var input = inputs.Data;
        //string[] arr = input.WorkBookCode.Split(',');  

        if (input.NodeId?.ToUpper()== "COMMONTEMPLATE")
        {
            input.Opt3 = "1";
            input.NodeId = null;
        }

        var data = await repository.Queryable<BIWorkbook>()
            .WhereIF(
                input.WorkBookCode.IsNotNullOrEmpty(),
                x => x.WorkBookCode.Contains(input.WorkBookCode))
            .WhereIF(
                !input.WorkBookName.IsNullOrEmpty(),
                x => x.WorkBookName.Contains(input.WorkBookName))
            .WhereIF(
                true,
                x => x.DeleteFlag == "N")
            .WhereIF(
                input.Opt3.IsNotNullOrEmpty(), 
                t => t.Opt3 == input.Opt3)
            .WhereIF(
                input.NodeId.IsNotNullOrEmpty(), 
                t => t.Opt2 == input.NodeId)
            .OrderBy(inputs.OrderField)
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);

        return new PageEntity<IEnumerable<BIWorkbook>>
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
    /// 更改工作簿
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<string> ModifyAsync(BIWorkbookInput input)
    {
        // 用户更改工作簿名称或工作簿编码，编码不能与其他工作簿编码重复
        if (input.Id.IsEmpty())
        {
            return "工作簿编码不能为空！";
        }
        var inputentitys = await repository.Queryable<BIWorkbook>().Where(x => x.WorkBookCode == input.WorkBookCode && x.DeleteFlag == "N" && x.Id != input.Id).ToListAsync();
        if (inputentitys.Any())
            return "工作簿编码重复！";

        // BIWorkbook 父表更新，BiCalcField，BiFilterField,BiCalcField 子表先删除记录再新增

        #region 修改父表记录        
        var entity = input.MapTo<BIWorkbook>();

        // 从markstyle 中解析标记实体类
        if(input.MarkStyle != null)
        {
            var marksJson = JToken.Parse(entity.MarkStyle);
            input.MarkItems = new List<BiMarkField>();
        }
        else
        {
            input.MarkItems = new List<BiMarkField>();
        }
        

        // 修改人
        entity.Modify(entity.Id, input.CurrentUser);
        #endregion

        #region 删除子表记录 
        var deleteCalists = await repository.Queryable<BiCalcField>()
               .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId==input.Id).ToListAsync();
        var deleteFilists = await repository.Queryable<BiFilterField>()
               .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId==input.Id).ToListAsync();
        var deleteMarklists = await repository.Queryable<BiMarkField>()
               .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.WorkbookId==input.Id).ToListAsync();
        #endregion

        #region 新增子表记录
        foreach (var item in input.CalcItems)
        {
            item.Create(input.CurrentUser);
            item.WorkbookId = input.Id;
            item.SortValueJson = item.SortValue == null ? null : JsonConvert.SerializeObject(item.SortValue);
        }
        for (int i = 0; i < input.FilterItems.Count; i++)
        {
            input.FilterItems[i].Create(input.CurrentUser);
            input.FilterItems[i].WorkbookId = entity.Id;
            input.FilterItems[i].OrderBy = i;
        }
        if (input.MarkItems != null)
        {
            foreach (var item in input.MarkItems)
            {
                item.Create(input.CurrentUser);
                item.WorkbookId = input.Id;
                item.SortValueJson = item.SortValue == null ? null : JsonConvert.SerializeObject(item.SortValue);
            }
        }
        
        #endregion

        try
        {
            // 删除子表BiCalcField
            await repository.Deleteable<BiCalcField>(deleteCalists).ExecuteCommandAsync();
            // 删除子表BiFilterField
            await repository.Deleteable<BiFilterField>(deleteFilists).ExecuteCommandAsync();
            // 删除子表BiMarkField
            await repository.Deleteable<BiMarkField>(deleteMarklists).ExecuteCommandAsync();
            // 更改父表
            await repository.Updateable<BIWorkbook>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
            // 新增子表BiCalcField
            await repository.Insertable<BiCalcField>(input.CalcItems).ExecuteCommandAsync();
            // 新增子表BiFilterField
            await repository.Insertable<BiFilterField>(input.FilterItems).ExecuteCommandAsync();
            // 新增子表BiMarkField
            await repository.Insertable<BiMarkField>(input.MarkItems).ExecuteCommandAsync();


        }
        catch (Exception ex)
        {
            repository.Ado.RollbackTran();
            return ex.Message;
        }
         
        return "OK";
    }
    /// <summary>
    /// 预览查询工作簿
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(string, DataTable)> previewAsync(BIWorkbookInput input)
    {
        // 工作簿配置相关信息
        var workbook = await repository.Queryable<BIWorkbook>()
            .WhereIF(
                input.Id.IsNotNullOrEmpty(),
                x => x.Id.Contains(input.Id)).FirstAsync();    
        
        // 根据用户输入的筛选条件 生成执行的sql
        var result = await filterServices.BIFilter(input);
        if (result.Item1 != "OK")
            return (result.Item1, null);        
        string wheresql = result.Item2;
        var sql = workbook.DynamicSql.Replace("{WHERE}", wheresql);
        sql = sql.Replace("@", ",");

        // 执行查询
        var dataset = await repository.Queryable<BiDataset>().Where(x => x.Id == input.DatasetId).ToListAsync();
        var datsSource = await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataset.First().SourceCode).ToListAsync();
        var dbItem = dbEngineService.GetRepository(datsSource.First().SourceType, datsSource.First().SourceConnect);
        var data = await dbItem.Item1.Ado.GetDataTableAsync(sql);

        return ("OK",data);
    }
    /// <summary>
    /// 复制工作簿
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    //public async Task<string> copyAsync(BIWorkbookInput input)
    //{
    //    input.markItems = input.markItems ?? new List<BiMarkField>();
    //    var inputentitys = await repository.Queryable<BIWorkbook>().Where(x => x.WorkBookCode == input.WorkBookCode && x.deleteFlag == "N").ToListAsync();
    //    if (inputentitys.Any())
    //        return "请勿重复插入！";

    //    var entity = input.MapTo<BIWorkbook>();

    //    // sql生成 
    //    try
    //    {
    //        // 判空
    //        if (input.filterItems == null && !input.filterItems.Any())
    //        {
    //            return "请选择需要筛选的字段";
    //        }
    //        if ((input.markItems == null || !input.markItems.Any()) && (input.calcItems == null || !input.filterItems.Any()))
    //        {
    //            return "请选择需要计算的字段或者标记字段";
    //        }
    //        StringBuilder sb = new();
    //        (string, string) result;

    //        // select  调用三个service 
    //        result = await aggregationServices.AnalysisCalcSql(input);
    //        if (result.Item1 != "OK")
    //        {
    //            entity.dynamicSql = $"ERROR {result.Item1}";
    //            goto on;
    //        }
    //        sb.Append(result.Item2);

    //        result = await markServices.AnalysisMarkSql(input);
    //        if (result.Item1 != "OK")
    //        {
    //            entity.dynamicSql = $"ERROR {result.Item1}";
    //            goto on;
    //        }

    //        if (sb.Length > 8 && result.Item2.Length > 0)
    //            sb.Append(',');

    //        sb.Append(result.Item2);

    //        // from    执行service查询 表关系链接
    //        result = await rService.AnalysisRelation(input);
    //        if (result.Item1 != "OK")
    //        {
    //            entity.dynamicSql = $"ERROR {result.Item1}";
    //            goto on;
    //        }
    //        sb.Append(result.Item2);

    //        // where   筛选器条件用替换符
    //        sb.Append("{WHERE}");

    //        // 添加groupby 查询函数
    //        result = aggregationServices.AnalysisAggregate(input);
    //        if (result.Item1 != "OK")
    //        {
    //            entity.dynamicSql = $"ERROR {result.Item1}";
    //            goto on;
    //        }
    //        sb.Append(result.Item2);
    //        entity.dynamicSql = sb.ToString();
    //    }
    //    catch (Exception ex)
    //    {
    //        entity.dynamicSql = ex.Message;
    //    }

    //   // 生成主键ID
    //   on: entity.Create(input.CurrentUser);

    //    foreach (var item in input.calcItems)
    //    {
    //        item.Create(input.CurrentUser);
    //        item.WorkbookId = entity.Id;
    //    }
    //    foreach (var item in input.filterItems)
    //    {
    //        item.Create(input.CurrentUser);
    //        item.WorkbookId = entity.Id;
    //    }
    //    foreach (var item in input.markItems)
    //    {
    //        item.Create(input.CurrentUser);
    //        item.WorkbookId = entity.Id;
    //    }

    //    // 开启事务执行插入
    //    try
    //    {
    //        repository.Ado.BeginTran();
    //        await repository.Insertable<BIWorkbook>(entity).ExecuteCommandAsync();
    //        await repository.Insertable<BiCalcField>(input.calcItems).ExecuteCommandAsync();
    //        await repository.Insertable<BiFilterField>(input.filterItems).ExecuteCommandAsync();
    //        await repository.Insertable<BiMarkField>(input.markItems).ExecuteCommandAsync();
    //        repository.Ado.CommitTran();
    //    }
    //    catch (Exception ex)
    //    {
    //        repository.Ado.RollbackTran();
    //        return ex.Message;
    //    }
    //    return "OK";
    //}

}
