using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

public class BIFilterServices : IBIFilterServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;
    /// <summary>
    /// 数据库sql语句引擎
    /// </summary>
    public IDbEngineServices dbEngineServices;
    /// <summary>
    /// sql处理引擎
    /// </summary>
    private IDbEngineServices dbEngine;
    /// <summary>
    /// 自定义查询功能
    /// </summary>
    private DataSourceServices dataSourceService;

    public BIFilterServices(    ISqlSugarClient _sqlSugarClient
                                , IDbEngineServices dbEngineServices           
                                , IDbEngineServices dbService
                                , DataSourceServices dataSourceService)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.dbEngine = dbService;
        this.dbEngineServices = dbEngineServices;
        this.dataSourceService = dataSourceService;
    }

    public async Task<(string,string)> BIFilter(BIWorkbookInput input)
    {
        // 读取前台传入的List<BiFilterField> filterItems
        // 获取数据源
        var dataset = (await repository.Queryable<BiDataset>().Where(x => x.Id == input.DatasetId).ToListAsync()).FirstOrDefault();
        var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataset.SourceCode && x.DeleteFlag == 0).ToListAsync()).FirstOrDefault();

        BiFilterField filters = new BiFilterField();        
        StringBuilder wheresql = new StringBuilder(" WHERE ");

        foreach (var fItem in input.FilterItems)
        {
            if( !string.IsNullOrEmpty(fItem.FilterValue))
            {
                // 遍历表标签、筛选字段、值数组 LabelName ColumnName FilterValue
                filters.LabelName = fItem.LabelName;
                filters.ColumnName = fItem.ColumnName;
                filters.FilterValue = fItem.FilterValue;

                //判断数据类型,除时间类型用< > between...and...，其他都用in
                if (fItem.ColumnType == "DATE" || fItem.ColumnType == "TIMESTAMP")
                {
                    wheresql.Append(filters.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                    wheresql.Append(".");
                    wheresql.Append(filters.ColumnName);
                    //前台时间控件  starttime  endtime
                    wheresql.Append(" BETWEEN TO_DATE('" + filters.FilterValue.Split(",")[0] + "','YYYY-MM-DD HH24:MI:SS') AND TO_DATE('" + filters.FilterValue.Split(",")[1] + "','YYYY-MM-DD HH24:MI:SS')");

                }
                else
                {
                    var arr = filters.FilterValue.Split(',');
                    if(arr.Length <= 1000 || dataSource.SourceType != "Oracle")
                    {
                        wheresql.Append(filters.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                        wheresql.Append(".");
                        wheresql.Append(filters.ColumnName);
                        wheresql.Append(" IN " + "(" + "'");
                        wheresql.Append(filters.FilterValue.Replace(",", "','"));
                        wheresql.Append("')");
                    }
                    else
                    {
                        wheresql.Append('(');
                        for(int i = 0; i < arr.Length; i++)
                        {
                            if(i == 0)
                            {
                                wheresql.Append(filters.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                                wheresql.Append(".");
                                wheresql.Append(filters.ColumnName);
                                wheresql.Append(" IN ");
                                wheresql.Append(" ( ");
                            }

                            wheresql.Append('\'');
                            wheresql.Append(arr[i]);
                            wheresql.Append('\'');
                            wheresql.Append(',');

                            if (i == 0)
                                continue;

                            if (i == arr.Length - 1 || (i + 1) % 1000 == 0)
                            {
                                wheresql = wheresql.Remove(wheresql.Length-1 ,1);
                                wheresql.Append(')');
                                wheresql.Append(' ');
                            }

                            if (i != arr.Length - 1 && (i + 1) % 1000 == 0)
                            {
                                wheresql.Append(" OR ");
                                wheresql.Append(filters.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                                wheresql.Append(".");
                                wheresql.Append(filters.ColumnName);
                                wheresql.Append(" IN ");
                                wheresql.Append(" ( ");
                            }
                        }
                        wheresql.Append(')');
                        wheresql.Append(' ');
                    }
                }
                wheresql.Append(" AND ");
            }
        }
        wheresql.Remove(wheresql.Length-4,4);
        return ("OK", wheresql.ToString());
    }

    public async Task<(string, PageEntity<List<string>>)> selectValue(PageEntity<ColumnInfo> input)
    {
        string sql;
        //获取数据源  datasetId ==> datasource
        var dataSet = (await repository.Queryable<BiDataset>().Where(x => x.Id == input.Data.DatasetId && x.DeleteFlag == "N" ).ToListAsync()).FirstOrDefault();
        var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataSet.SourceCode && x.DeleteFlag == 0 ).ToListAsync()).FirstOrDefault();
        if (dataSource == null)
            return("数据源不存在", null);
        var engine = dbEngine.GetRepository(dataSource.SourceType, dataSource.SourceConnect);


        // 字段只会有1个 
        // 不同库的SQL语法不同，待拓   展dbEngine里的方法
       
        // 根据nodeid查询从BI_DATASET_NODE获取真正的tablename
        if(input.Data.ColumnType == "1")
        {
            var dataSets = (await repository.Queryable<BiDatasetNode>().Where(x => x.NodeLabel == input.Data.LabelName && x.DeleteFlag == "N" ).ToListAsync()).FirstOrDefault();
            sql = $@"SELECT DISTINCT {input.Data.ColumnName} FILTERVALUE FROM {dataSets.TableName} ";
        }
        else if(input.Data.ColumnType == "2")
        {
            sql = "";
        }
        else
        {
            var dataSets = (await repository.Queryable<BiDatasetNode>().Where(x => x.Id == input.Data.NodeId && x.DeleteFlag == "N" ).ToListAsync()).FirstOrDefault();
            sql = $@"SELECT DISTINCT {input.Data.ColumnName} FILTERVALUE FROM {dataSets.TableName} ";
        }

        sql = dbEngine.sqlPageRework(sql,0,1000, dataSource.SourceType);
        var result = await dataSourceService.testDB(new DataCollectDBTest
        {
            SourceCode = dataSource.SourceCode,
            SourceType = dataSource.SourceType,
            SourceConnect = dataSource.SourceConnect,
            DynSql = sql,
            SearchAll = true
        });

        List<string> datalist = result.Item1.Select().Select(x => x[0].ToString()).ToList();

        //分页查询
        //RefAsync<int> total = 0;

        return ("OK",new  PageEntity<List<string>>
        {
            PageIndex = input.PageIndex,
            Ascending = input.Ascending,
            PageSize = input.PageSize,
            OrderField = input.OrderField,
            Total = (long)datalist.Count,
            Data = datalist
        });
    }
}
