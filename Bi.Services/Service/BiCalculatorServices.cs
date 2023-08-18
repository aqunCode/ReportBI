using Bi.Core.Caches;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Cryptography;
using SqlSugar;
using System.Collections;
using System.Data;
using System.Text;
using Bi.Core.Helpers;
using System.Net.Http.Headers;

namespace Bi.Services.Service;

public class BiCalculatorServices : IBiCalculatorServices
{

    private readonly ILogger<BiCalculatorServices> logger;
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;
    /// <summary>
    /// sql处理引擎
    /// </summary>
    private IDbEngineServices dbEngineService;
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
    /// 标记服务接口
    /// </summary>
    private readonly IBIMarkServices markServices;
    /// <summary>
    /// redis 缓存
    /// </summary>
    private ICache redisCache;
    /// <summary>
    /// 数据源
    /// </summary>
    private DataSourceServices dataSourceService;


    /// <summary>
    /// DataSet 构造函数
    /// </summary>
    public BiCalculatorServices(IBinaryTreeServices service
                                , IBIFilterServices filterServices
                                , ISqlSugarClient _sqlSugarClient
                                , IDbEngineServices dbService
                                , IAggregationServices aggregationServices
                                , IBIMarkServices markServices
                                , ILogger<BiCalculatorServices> logger
                                , ICache redisCache
                                , DataSourceServices dataSourceService)
    {
        this.rService = service;
        this.filterServices = filterServices;
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
        this.dbEngineService = dbService;
        this.aggregationServices = aggregationServices;
        this.markServices = markServices;
        this.logger = logger;
        this.redisCache = redisCache.UseKeyPrefix("[BI-execute]");
        this.dataSourceService = dataSourceService;
    }

    public async Task<(string, DataTable)> execute(BIWorkbookInput input)
    {
        // 添加redis缓存机制
        input.MessageId = "";
        string str = JsonConvert.SerializeObject(input);
        string redisKey = "";

        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(str);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            redisKey = sb.ToString();
        }
        #region 此处记录模型的访问记录以及不同条件的访问时间
        try
        {
            if (input.Id.IsNotNullOrEmpty())
            {
                await redisCache.ZIncrByAsync(string.Concat(new string[] { input.Id,"_", DateTime.Now.ToString("yyyyMMdd") }), redisKey);
                await redisCache.ListRightPushAsync(string.Concat(redisKey, DateTime.Now.ToString("yyyyMMdd")), DateTime.Now.ToString("yyyyMMddHHmmss"));
            }
        }
        catch (Exception ex) { logger.LogInformation($":ERROR {ex}"); }


        #endregion

        if (await redisCache.KeyExistsAsync(redisKey))
        {
            var result = ("OK", await redisCache.GetAsync<DataTable>(redisKey));
            logger.LogInformation("获取缓存成功");
            // 使用Task.Factory.StartNew来开启一个后台异步任务更新
            #pragma warning disable CS4014
            Task.Factory.StartNew(() =>
            {
                // 不使用await关键字，让异步方法在后台运行
                FlushRedisCache(input, redisKey).ConfigureAwait(false);
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return result;
        }
        else
        {
            var result = await Analyse(input);
            try
            {
                await redisCache.SetAsync(redisKey, result.Item2, 43200);
                // 设置数据刷新小黑屋
                await redisCache.SetAsync(string.Concat(redisKey,"timeOut"),"", 300);
            }
            catch (Exception ex) { logger.LogInformation($":ERROR {ex}"); }

            return result;
        }
    }

    /// <summary> 
    /// 利用异步方法不会等待的特性刷新缓存数据
    /// </summary>
    /// <param name="input"></param>
    /// <param name="redisKey"></param>
    async Task FlushRedisCache(BIWorkbookInput input, string redisKey)
    {
        if (await redisCache.KeyExistsAsync(string.Concat(redisKey, "timeOut")))
        {
            logger.LogInformation($"{redisKey}:小黑屋反省中");
            return;
        }
        (string, DataTable) result;
        try
        {
            await redisCache.SetAsync(string.Concat(redisKey, "timeOut"), "", 300);
            result = await Analyse(input);
        }catch(Exception ex)
        {
            await redisCache.RemoveAsync(redisKey);
            logger.LogInformation($"异步缓存刷新失败，数据库查询异常如下:ERROR {ex}");
            return;
        }
        try
        {
            await redisCache.SetAsync(redisKey, result.Item2, 43200);
            // 设置数据刷新小黑屋
            await redisCache.SetAsync(string.Concat(redisKey, "timeOut"), "", 300);
            logger.LogInformation($"{redisKey}:异步缓存刷新成功，小黑屋刷新成功");
        }
        catch (Exception ex) { logger.LogInformation($"[{DateTime.Now}]:ERROR {ex}"); }
    }

    private async Task<(string, DataTable)> Analyse(BIWorkbookInput input)
    {
        (string, string) result;
        StringBuilder sb = new();
        List<string> chilList = new();

        // 判空 挑拣
        if (input.FilterItems == null   && !input.FilterItems.Any() )
        {
            return ("请选择需要筛选的字段", null);
        }
        if ( ( input.MarkItems == null || !input.MarkItems.Any()) && (input.CalcItems == null || !input.FilterItems.Any()) )
        {
            return ("请选择需要计算的字段或者标记字段", null);
        }

        // 调用三个service  
        // select  
        result = await aggregationServices.AnalysisCalcSql(input);
        if (result.Item1 != "OK")
            return (result.Item1, null);

        // 收集子sql
        if (result.Item2.Contains("::"))
        {
            var arr = result.Item2.Split("::");
            for (int i = 0; i < arr.Length; i++)
            {
                if (i == arr.Length - 1)
                    sb.Append(arr[i]);
                else
                    chilList.Add(arr[i]);
            }
        }
        else
        {
            sb.Append(result.Item2);
        }
            

        result = await markServices.AnalysisMarkSql(input);
        if (result.Item1 != "OK")
            return (result.Item1, null);

        // 判断行列是否有要查询的字段
        if (sb.Length > 8 && result.Item2.Length > 0)
            sb.Append('@');

        // 收集子sql
        if (result.Item2.Contains("::"))
        {
            var arr = result.Item2.Split("::");
            for(int i = 0; i < arr.Length; i++)
            {
                if(i == arr.Length-1)
                    sb.Append(arr[i]);
                else
                    chilList.Add(arr[i]);
            }
        }
        else
        {
            sb.Append(result.Item2);
        }

        // 这里暂存sql拼接数据，以备生成groupBy 函数
        string groupBySql = sb.ToString();


        // from    执行service查询 表关系链接
        result = await rService.AnalysisRelation(input);
        if (result.Item1 != "OK")
            return (result.Item1, null);
        sb.Append(result.Item2);

        //拼接sql
        foreach(var item in chilList)
        {
            string[] arr = item.Split(':');
            sb.Append(" LEFT JOIN (SELECT ");
            sb.Append(arr[2]);
            sb.Append(result.Item2);
            sb.Append(arr[3]);
            sb.Append(" ) ");
            sb.Append(arr[1]);
        }

        // where   筛选器条件
        result = await filterServices.BIFilter(input);
        if (result.Item1 != "OK")
            return (result.Item1, null);
        sb.Append(result.Item2);

        // 这里开始生成 group by函数
        string[] groupByArr = groupBySql.Replace(" SELECT ", " ").Split('@');

        // 设定所有要剔除的函数
        sb.Append(" GROUP BY ");
        foreach (var item in groupByArr)
        {
            if (dbEngineService.checkAggregate(item))
                continue;
            sb.Append(item.AsSpan(0, item.LastIndexOf(' ')));
            sb.Append(',');
        }
        sb = sb.Remove(sb.Length-1,1);

        // 这里开始生成排序字段
        //var status = await GeneralSortBy(sb,groupByArr, input);
        // 去除结尾逗号
        sb = sb.Remove(sb.Length - 1, 1);
        sb = sb.Replace('@', ',');
        // 执行查询 
        var dataset = await repository.Queryable<BiDataset>().Where(x => x.Id == input.DatasetId).ToListAsync();
        var dataSource = await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataset.First().SourceCode).ToListAsync();

        var dbItem = dbEngineService.GetRepository(dataSource.First().SourceType, dataSource.First().SourceConnect);

        logger.LogInformation($"[{DateTime.Now}] BI 生成sql:{sb}");

        // 此处修改原因  Number（38，0）此类数据或者精度更高的小数，用datatable 无法接收，
        // 抛出异常  Specified cast is not valid，故做以下修改
        DataTable dt = new DataTable();
        if (dataSource.First().SourceType == "Spark")
        {
            var tmp = await dataSourceService.testDB(new DataCollectDBTest
            {
                SourceCode = dataSource.First().SourceCode,
                SourceType = dataSource.First().SourceType,
                SourceConnect = dataSource.First().SourceConnect,
                DynSql = sb.ToString(),
                SearchAll = true
            });
            dt = tmp.Item1;
        }
        else
        {
            IDataReader reader = null;
            try
            {
                reader = await dbItem.Item1.Ado.GetDataReaderAsync(sb.ToString());
                int columnCount = reader.FieldCount;
                bool titleFlag = true;
                while (reader.Read())
                {
                    if (titleFlag)
                    {
                        for (int i = 0; i < columnCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            if (titleFlag)
                            {
                                dt.Columns.Add(columnName);
                            }
                        }
                        titleFlag = false;
                    }
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < columnCount; i++)
                    {
                        var typeStr = reader.GetDataTypeName(i);

                        if (typeStr == "Decimal")
                        {
                            if (reader.IsDBNull(i))
                                dr[i] = null;
                            else
                                dr[i] = Decimal.Round(Decimal.Parse(reader.GetString(i)), 5);
                        }
                        else
                            dr[i] = reader.GetValue(i);
                    }
                    dt.Rows.Add(dr);
                }
                reader.Close();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                dbItem.Item1.Close();
            }
        }

        // 这里对DataTable 进行排序，替代之前的sql排序
        dt = await sortByCustomer(dt, groupByArr, input);


        //var data = await dbItem.Item1.Ado.GetDataTableAsync(sb.ToString());
        if (dt.Rows.Count > input.MaxNumber)
        {
            dt = dt.Select().Take(input.MaxNumber).CopyToDataTable();
            return ("OK:查询条目数超过设置，请添加筛选器或增大查询颗粒度", dt);
        }
        // 返回处理完的数据 
        return ("OK", dt);
    }

    private async Task<DataTable> sortByCustomer(DataTable dt, string[] groupByArr, BIWorkbookInput input)
    {
        //剔除SortBy=0和SortBy是空的
        var calcSorts = input.CalcItems.Where(x => !string.IsNullOrEmpty(x.SortBy) && x.SortBy != "0");
        // 标记区字段的排序是否因联动而重复？
        var markSorts = input.MarkItems.Where(x => !string.IsNullOrEmpty(x.SortBy) && x.SortBy != "0");

        if (calcSorts.Any() || markSorts.Any())
        {
            var dataset = (await repository.Queryable<BiDataset>().Where(x => x.Id == input.DatasetId).ToListAsync()).FirstOrDefault();
            var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataset.SourceCode && x.DeleteFlag == 0).ToListAsync()).FirstOrDefault();

            int sortCount = calcSorts.Count() + markSorts.Count();
            // 先确定行和列的优先顺序，标记肯定是最后排序  y是列，x是行，x里面有指标，y里没有指标时先排序y，其余情况先排序x
            string orderStr;
            if (input.CalcItems.Where(x => x.Axis == "x" && (!string.IsNullOrEmpty(x.CalculatorFunction) || x.DataType == "Number")).Any()
                    && input.CalcItems.Where(x => x.Axis == "y" && (!string.IsNullOrEmpty(x.CalculatorFunction) || x.DataType == "Number")).Count() == 0)
                orderStr = "y";
            else
                orderStr = "x";

            int index = 0;

            DataTableHelper dber = new DataTableHelper(dt);
            // 第一阶sql生成(行或者列)
            sortCount = sortByColumn(index++, dber, groupByArr, orderStr, sortCount, input, dataSource.SourceType);
            if (sortCount == 0)
                return dber.GetValue();

            // 第二阶sql生成(行或者列) index=1跳过指标
            orderStr = orderStr == "x" ? "y" : "x";
            sortCount = sortByColumn(index++, dber, groupByArr, orderStr, sortCount, input, dataSource.SourceType);
            if (sortCount == 0)
                return dber.GetValue();

            // 第三阶sql生成(标记)  
            sortCount = sortByColumn(index++, dber, groupByArr, "", sortCount, input, dataSource.SourceType);
            if (sortCount == 0)
                return dber.GetValue();

            // 第四阶sql生成(行或者列)将指标字段作为最后排序  index=3跳过指标所在行/列的维度
            sortCount = sortByColumn(index++, dber, groupByArr, orderStr, sortCount, input, dataSource.SourceType);
            if (sortCount == 0)
                return dber.GetValue();

            return dber.GetValue();
        }
        else
        {
            return dt;
        }
    }

    private int sortByColumn(int index, DataTableHelper dber, string[] groupByArr, string orderStr, int sortCount, BIWorkbookInput input, string? sourceType)
    {
        if (!string.IsNullOrEmpty(orderStr))
        {
            // 行列区遍历所有字段，不跳过sortby为空的字段
            var tmp = input.CalcItems.Where(x => x.Axis == orderStr).Where(x => !string.IsNullOrEmpty(x.SortBy)).OrderBy(x => x.OrderBy).ToList();
            foreach (var item in tmp)
            {
                // 这里执行换壳，将标记中和行列相同的字段属性换壳到当前item  --针对标记中的任一属性（不只是颜色）
                // 保证标记排序不为空，再换壳。假如标记排序为空，不需要换壳 （因为通常只对颜色排序）
                var marks = input.MarkItems.Where(x => x.NodeId == item.NodeId && x.ColumnName == item.ColumnName && !string.IsNullOrEmpty(x.SortBy) && x.SortBy != "0");
                string SortBy;
                string SortType;
                List<SortField> SortValue;
                string CalculatorFunction;
                string DataType;
                string Axis;
                string OrderBy;
                if (marks.Any())
                {
                    var mark = marks.First();
                    SortBy = mark.SortBy;
                    SortType = mark.SortType;
                    SortValue = mark.SortValue;
                    CalculatorFunction = mark.CalculatorFunction;
                    DataType = mark.DataType;
                    Axis = mark.Axis;
                    OrderBy = mark.OrderBy;

                    input.MarkItems.RemoveWhere(x => x.NodeId == item.NodeId && x.ColumnName == item.ColumnName);
                    sortCount = sortCount - 2;
                }
                else
                {
                    SortBy = item.SortBy;
                    SortType = item.SortType;
                    SortValue = item.SortValue;
                    CalculatorFunction = item.CalculatorFunction;
                    DataType = item.DataType;
                    Axis = item.Axis;
                    OrderBy = item.OrderBy.ToString();
                    if (!string.IsNullOrEmpty(SortBy) && SortBy != "0")
                        sortCount--;
                }

                string dataType = (DataType == "String" || DataType == "DateTime") ? "String" : "Number";

                // 第一次跳过指标
                if (index == 1 && dataType == "Number")
                    continue;

                // 第二次跳过维度
                if (index == 3 && dataType == "String")
                    continue;

                string code = string.Concat(Axis, OrderBy).ToUpper();

                dber.Sortby(code, 
                            SortBy, 
                            SortValue.Select(x=>x.Value).ToArray());

                if (sortCount == 0)
                    return sortCount;
            }
        }
        else
        {
            // 标记模块，跳过所有SortBy为空的字段
            var tmps = input.MarkItems.Where(x => x.Axis == "z" && !string.IsNullOrEmpty(x.SortBy) && x.SortBy != "0").OrderBy(x => x.OrderBy).ToList();
            ArrayList columnnames = new ArrayList(10);
            foreach (var item in tmps)
            {
                // 判断是否为重复字段。重复字段无需再排序
                if (columnnames.Contains(item.ColumnName))
                {
                    sortCount--;
                    continue;
                }

                // 将已遍历的字段加入数组
                columnnames.Add(item.ColumnName);

                string dataType = (!string.IsNullOrEmpty(item.CalculatorFunction) || item.DataType == "Number") ? "Number" : "String";
                string code = string.Concat(item.Axis, item.OrderBy).ToUpper();
                
                dber.Sortby(code,
                            item.SortBy,
                            item.SortValue.Select(x => x.Value).ToArray());
                // if (!string.IsNullOrEmpty(item.SortBy)&& item.SortBy != "0")  //筛选sortby不为空后，此条件肯定是TRUE
                sortCount--;
                if (sortCount == 0)
                    return sortCount;
            }
        }
        return sortCount;
    }

    // 生成排序字段
    private async Task<int> GeneralSortBy(StringBuilder sb, string[] groupByArr, BIWorkbookInput input)
    {
        //剔除SortBy=0和SortBy是空的
        var calcSorts = input.CalcItems.Where(x => !string.IsNullOrEmpty(x.SortBy) && x.SortBy!="0");
        // 标记区字段的排序是否因联动而重复？
        var markSorts = input.MarkItems.Where(x => !string.IsNullOrEmpty(x.SortBy) && x.SortBy != "0" );

        if (calcSorts.Any() || markSorts.Any())
        {
            var dataset = (await repository.Queryable<BiDataset>().Where(x => x.Id == input.DatasetId).ToListAsync()).FirstOrDefault();
            var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataset.SourceCode && x.DeleteFlag == 0).ToListAsync()).FirstOrDefault();
                        
            int sortCount = calcSorts.Count() + markSorts.Count();
            // 先确定行和列的优先顺序，标记肯定是最后排序  y是列，x是行，x里面有指标，y里没有指标时先排序y，其余情况先排序x
            string orderStr;
            if (input.CalcItems.Where(x => x.Axis == "x" && ( !string.IsNullOrEmpty(x.CalculatorFunction) || x.DataType == "Number")).Any()
                    && input.CalcItems.Where(x => x.Axis == "y" && (!string.IsNullOrEmpty(x.CalculatorFunction) || x.DataType == "Number")).Count() == 0)
                orderStr = "y";
            else
                orderStr = "x";
            sb.Append(" ORDER BY ");

            int index = 0;
            // 第一阶sql生成(行或者列)
            sortCount = generateField(index++,sb, groupByArr,orderStr, sortCount, input, dataSource.SourceType);
            if (sortCount == 0)
                return 1;

            // 第二阶sql生成(行或者列) index=1跳过指标
            orderStr = orderStr == "x"?"y":"x";
            sortCount = generateField(index++, sb, groupByArr, orderStr, sortCount, input, dataSource.SourceType);
            if (sortCount == 0)
                return 1;

            // 第三阶sql生成(标记)  
            sortCount = generateField(index++, sb, groupByArr, "", sortCount, input, dataSource.SourceType);
            if (sortCount == 0)
                return 1;

            // 第四阶sql生成(行或者列)将指标字段作为最后排序  index=3跳过指标所在行/列的维度
            sortCount = generateField(index++, sb, groupByArr, orderStr, sortCount, input, dataSource.SourceType);
            if (sortCount == 0)
                return 1;
        }
        return 1;
    }

    private int generateField(int index,StringBuilder sb, string[] groupByArr, string orderStr,int sortCount, BIWorkbookInput input,string sourceType)
    {

        if (!string.IsNullOrEmpty(orderStr))
        {
            // 行列区遍历所有字段，不跳过sortby为空的字段
            var tmp = input.CalcItems.Where(x => x.Axis == orderStr).Where(x=>!string.IsNullOrEmpty(x.SortBy)).OrderBy(x => x.OrderBy).ToList();
            foreach(var item in tmp)
            {
                // 这里执行换壳，将标记中和行列相同的字段属性换壳到当前item  --针对标记中的任一属性（不只是颜色）
                // 保证标记排序不为空，再换壳。假如标记排序为空，不需要换壳 （因为通常只对颜色排序）
                var marks = input.MarkItems.Where(x => x.NodeId == item.NodeId && x.ColumnName == item.ColumnName && !string.IsNullOrEmpty(x.SortBy) && x.SortBy!="0");
                string SortBy ;
                string SortType ;
                List<SortField> SortValue ;
                string CalculatorFunction ;
                string DataType;
                string Axis;
                string OrderBy;
                if (marks.Any())
                {
                    var mark = marks.First();
                    SortBy = mark.SortBy;
                    SortType = mark.SortType;
                    SortValue = mark.SortValue;
                    CalculatorFunction = mark.CalculatorFunction;
                    DataType = mark.DataType;
                    Axis = mark.Axis;
                    OrderBy = mark.OrderBy;

                    input.MarkItems.RemoveWhere(x => x.NodeId == item.NodeId && x.ColumnName == item.ColumnName);
                    sortCount = sortCount-2;
                }
                else
                {
                    SortBy = item.SortBy;
                    SortType = item.SortType;
                    SortValue = item.SortValue;
                    CalculatorFunction = item.CalculatorFunction;
                    DataType = item.DataType;
                    Axis = item.Axis;
                    OrderBy = item.OrderBy.ToString();
                    if (!string.IsNullOrEmpty(SortBy) && SortBy != "0")
                        sortCount--;
                }

                string dataType = (DataType == "String" || DataType == "DateTime") ? "String" : "Number";
                
                // 第一次跳过指标
                if (index == 1 && dataType == "Number")
                    continue;

                // 第二次跳过维度
                if (index == 3 && dataType == "String")
                    continue;

                string code = string.Concat(Axis,OrderBy);
                var field = groupByArr.Where(x =>
                    {
                        var res = x.Substring(x.LastIndexOf(' '));
                        if (res.Trim() == code)
                            return true;
                        else
                            return false;
                    }).FirstOrDefault();

                sb.Append(' ');
                sb.Append(dbEngineService.showOrderBy(   SortBy
                                                        ,field.Substring(0, field.LastIndexOf(' '))
                                                        , dataType
                                                        , SortValue
                                                        ,sourceType));
                sb.Append(',');

                if (sortCount == 0)
                    return sortCount;
            }
        }
        else
        {
            // 标记模块，跳过所有SortBy为空的字段
            var tmps = input.MarkItems.Where(x => x.Axis == "z" && !string.IsNullOrEmpty(x.SortBy) && x.SortBy != "0").OrderBy(x => x.OrderBy).ToList();
            ArrayList columnnames = new ArrayList(10);
            foreach (var item in tmps)
            {
                // 判断是否为重复字段。重复字段无需再排序
                if (columnnames.Contains(item.ColumnName))
                {
                    sortCount--;
                    continue;
                }

                // 将已遍历的字段加入数组
                columnnames.Add(item.ColumnName);

                string dataType = (!string.IsNullOrEmpty(item.CalculatorFunction) || item.DataType == "Number") ? "Number" : "String";
                string code = string.Concat(item.Axis, item.OrderBy);
                var field = groupByArr.Where(x =>
                {
                    var res = x.Substring(x.LastIndexOf(' '));
                    if (res.Trim() == code)
                        return true;
                    else
                        return false;
                }).FirstOrDefault();

                sb.Append(' ');
                sb.Append(dbEngineService.showOrderBy(item.SortBy
                                                        , field.Substring(0, field.LastIndexOf(' '))
                                                        , dataType
                                                        , item.SortValue
                                                        , sourceType));
                sb.Append(',');
                // if (!string.IsNullOrEmpty(item.SortBy)&& item.SortBy != "0")  //筛选sortby不为空后，此条件肯定是TRUE
                sortCount--;
                if (sortCount == 0)
                    return sortCount;
            }
        }
        return sortCount;
    }

    #region 确定一个数组放几个元素
    /*private void splie(int n)
    {
        // 设定一个长度为10的数组
        int[] arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        
        List<int[]> list = new();
        int[] tmp = new int[n];

        for (int i = 0; i < arr.Length; i++)
        {
            // 当i摩于n为0，代表是第一个元素，或者上一个数组已经填满，需要添加数组到集合，创建一个新的数组
            if(i%n == 0)
            {
                list.Add(tmp);
                tmp = new int[n];
            }
            tmp[i % n] = arr[i];
        }
        if(arr.Length % n != 0)
            list.Add(tmp);
    }*/
    #endregion

    public async Task<(string, PageEntity<List<string>>)> selectValue(PageEntity<ColumnInfo> input)
    {
        var res=await filterServices.selectValue(input);
        return res;
    }

    public async Task<(string, List<string>)> getValueList(MarkValueInput input)
    {
        var res = await markServices.getValueList(input);
        return res;
    }

    /// <summary>
    /// datatable取前n行
    /// </summary>
    /// <param name="TopItem"></param>
    /// <param name="oDT"></param>
    /// <returns></returns>
    public static DataTable DtSelectTop(int TopItem, DataTable oDT)
    {
        if (oDT.Rows.Count < TopItem) return oDT;

        DataTable NewTable = oDT.Clone();
        DataRow[] rows = oDT.Select("1=1");
        for (int i = 0; i < TopItem; i++)
        {
            NewTable.ImportRow((DataRow)rows[i]);
        }
        return NewTable;
    }

    public async Task<string> sendWxMessage()
    {
        // 发送邮件
        string mailUrl = "http://10.191.19.65:8888/api/Mail/Send";
        using (HttpClient client = new HttpClient())
        {
            using (var formData = new MultipartFormDataContent())
            {
                // 添加文件
                var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes("E:\\aaa.txt"));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                formData.Add(fileContent, "files", "aaa.txt");

                // 添加其他键值对信息
                formData.Add(new StringContent("BU22MESs@luxshare-ict.com"), "fromUserEmail");
                formData.Add(new StringContent("flying@luxshare-ict.com"), "toUserEmail");
                formData.Add(new StringContent("message from test"), "subject");
                formData.Add(new StringContent("message from test"), "body");

                // 发送POST请求
                var response = await client.PostAsync(mailUrl, formData);
                if (response.IsSuccessStatusCode)
                {
                    // 处理成功的响应
                    var res = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(res);
                }
                else
                {
                    // 处理错误的响应
                    var res = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"请求失败，错误码：{response.StatusCode}，错误信息：{res}");
                }
            }
        }



        string url = "http://10.32.36.130:8000";
        var postData = new
        {
            appId = "1E3331D5833A0BAABCAC",
            appKey = "0x1e3331d50x7ab",
            extend = "",
            chatId = "reporttip002",
            text = "Message from NiFi 测试预警demo1demo"
        };
        var result = await HttpClientHelper.PostAsync(url, postData, @delegate: x => x.Timeout = TimeSpan.FromSeconds(30), httpClientName: "default");
        return "ok";
    }
}
