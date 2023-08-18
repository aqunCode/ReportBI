
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using SqlSugar;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Bi.Core.Extensions;
using System;
using System.Text.RegularExpressions;

namespace Bi.Services.Service;

public class BIMarkServices : IBIMarkServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;
    /// <summary>
    /// sql处理引擎
    /// </summary>
    private IDbEngineServices dbEngineServices;
    /// <summary>
    /// 自定义语法分析服务
    /// </summary>
    private IBiCustomerFieldServices syntaxServices;
    /// <summary>
    /// 二叉树from 表
    /// </summary>
    private IBinaryTreeServices binaryTreeServices;
    /// <summary>
    /// 自定义报表通用查询
    /// </summary>
    private DataSourceServices dataSourceService;

    public BIMarkServices(ISqlSugarClient _sqlSugarClient
                                , IDbEngineServices dbService
                                , IBiCustomerFieldServices syntaxServices
                                , IBinaryTreeServices binaryTreeServices
                                , DataSourceServices dataSourceService)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
        this.dbEngineServices = dbService;
        this.syntaxServices = syntaxServices;
        this.binaryTreeServices = binaryTreeServices;
        this.dataSourceService = dataSourceService;
    }

    public async Task<(string, string)> AnalysisMarkSql(BIWorkbookInput input)
    {
        StringBuilder markSql = new();
        StringBuilder sb = new();
        string fieldStr = "";

        var dataset = (await repository.Queryable<BiDataset>().Where(x => x.Id == input.DatasetId).ToListAsync()).FirstOrDefault();
        var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataset.SourceCode && x.DeleteFlag == 0).ToListAsync()).FirstOrDefault();

        IEnumerable<string> ienum = input.CalcItems.Where(x => x.ColumnType == "2").Select(x => x.ColumnName);

        foreach (var markField in input.MarkItems)
        {
            sb.Clear();
            fieldStr = "{columnName}";
            // 1 代表的是原始表字段的行列转换，2代表的是自定义字段 default 代表的是原始表字段
            switch (markField.ColumnType)
            {
                case "1":
                    BiCustomerField field = (await repository.Queryable<BiCustomerField>().Where(x => x.Id == markField.NodeId).ToListAsync()).FirstOrDefault();

                    sb.Append(field.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                    sb.Append('.');
                    sb.Append(field.FieldCode);
                    break;
                case "2":
                    /*if (ienum.Any() && ienum.Contains(markField.ColumnName))
                        continue;*/
                    // 这是自定义字段的解析函数 //cusField.FieldFunction, dataSource.sourceType, markField.ColumnName
                    var cusField = await repository.Queryable<BiCustomerField>().FirstAsync(x => x.Id == markField.NodeId);
                    var res = await syntaxServices.syntaxRules(new BiCustomerFieldInput
                    {
                        FieldFunction = cusField.FieldFunction,
                        DatasetId = input.DatasetId,
                        FieldCode = markField.ColumnName
                    });
                    if (res.Item1.IndexOf("ERROR") == 0)
                        return res;

                    if (res.Item1.Contains("::"))
                    {
                        var lastIndex = res.Item1.LastIndexOf("::");
                        sb.Append(res.Item1.Substring(lastIndex+2));
                        markSql.Insert(0, res.Item1.Substring(0, lastIndex+2));
                    }
                    else
                    {
                        sb.Append(res.Item1);
                    }
                    break;
                default:
                    // 这代表这原始表字段
                    sb.Append(markField.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                    sb.Append('.');
                    sb.Append(markField.ColumnName);
                    break;
            }
            fieldStr = fieldStr.Replace("{columnName}", sb.ToString());
            // 计算拖拽之后的计算函数
            markSql.Append(dbEngineServices.showFunction(markField.CalculatorFunction, dataSource.SourceType).Replace("{columnName}", fieldStr));
            markSql.Append(' ');
            markSql.Append(markField.Axis);
            markSql.Append(markField.OrderBy);
            markSql.Append('@');
        }
        int remove = markSql.LastIndexOf("@");
        if (remove != -1)
        {
            markSql = markSql.Remove(remove, 1);
        }
        return ("OK", markSql.ToString());
    }

    public async Task<(string, List<string>)> getValueList(MarkValueInput input)
    {
        string sql;
        string tableName;
        string tableRename;
        string functionStr;
        string groupBy = "";
        //获取数据源  datasetId ==> datasource
        var dataSet = await repository.Queryable<BiDataset>().FirstAsync(x => x.Id == input.MarkField.DatasetId && x.DeleteFlag == 0);
        var dataSource = await repository.Queryable<DataSource>().FirstAsync(x => x.SourceCode == dataSet.SourceCode && x.DeleteFlag == 0);
        
        // 创建数据库链接
        var engine = dbEngineServices.GetRepository(dataSource.SourceType, dataSource.SourceConnect);

        // 处理基本字段信息
        BiDatasetNode node;
        switch (input.MarkField.ColumnType)
        {
            case "1":
                var field = await repository.Queryable<BiCustomerField>().FirstAsync(x => x.Id == input.MarkField.NodeId && x.DeleteFlag == 0);
                node = await repository.Queryable<BiDatasetNode>().FirstAsync(x => x.NodeLabel == input.MarkField.LabelName && x.DeleteFlag == 0);
            
                if (field.TypeConvert == 0) // 维度转指标
                    functionStr = dbEngineServices.showFunction("toNumber", dataSource.SourceType);
                else                        // 指标转维度
                    functionStr = dbEngineServices.showFunction("toChar", dataSource.SourceType);

                functionStr = functionStr.Replace("{columnName}", input.MarkField.ColumnName);
                tableName = node.TableName;
                tableRename = input.MarkField.LabelName.Replace(".", "").Replace("(", "").Replace(")", "");
                break;
            case "2":
                var cusField = await repository.Queryable<BiCustomerField>().FirstAsync(x => x.Id == input.MarkField.NodeId);
                var res = await syntaxServices.syntaxRules(new BiCustomerFieldInput
                {
                    FieldCode = input.MarkField.ColumnName,
                    DatasetId = input.MarkField.DatasetId,
                    FieldFunction = cusField.FieldFunction
                });

                if (res.Item1.IndexOf("ERROR") == 0)
                    return (res.Item1,new List<string>());

                if (res.Item1.Contains("::"))
                {
                    var lastIndex = res.Item1.LastIndexOf("::");
                    string[] arr = res.Item1.Substring(0, lastIndex).Split(":");
                    functionStr = arr[2];
                    var marks = new List<BiMarkField>
                    {
                        input.MarkField
                    };
                    var fromResult = await binaryTreeServices.AnalysisRelation(new BIWorkbookInput
                    {
                        DatasetId = input.MarkField.DatasetId,
                        CalcItems = new List<BiCalcField>(),
                        FilterItems = new List<BiFilterField>(),
                        MarkItems = marks
                    });
                    if (fromResult.Item1 != "OK")
                        return (fromResult.Item1, null);
                    tableName = fromResult.Item2.Trim().TrimStart("FROM");
                    tableRename = "";

                }
                else
                {
                    node = await repository.Queryable<BiDatasetNode>().FirstAsync(x => x.NodeLabel == input.MarkField.LabelName && x.DeleteFlag == 0);
                    tableName = node.TableName;
                    functionStr = res.Item1;
                    tableRename = input.MarkField.LabelName.Replace(".", "").Replace("(", "").Replace(")", "");
                }
                break;
            default:
                node = await repository.Queryable<BiDatasetNode>().FirstAsync(x => x.Id == input.MarkField.NodeId && x.DeleteFlag == 0);
                tableName = node.TableName;
                tableRename = input.MarkField.LabelName.Replace(".", "").Replace("(", "").Replace(")", "");
                functionStr = string.Concat(tableRename,".",input.MarkField.ColumnName);
                break;
        }

        // 处理下拉函数信息
        (functionStr, groupBy)  = CalculatorFunction(functionStr, input.MarkField.CalculatorFunction, dataSource.SourceType, groupBy);
        
        // 根据筛选器添加where条件
        StringBuilder whereSql = new StringBuilder();
        whereSql.Append("WHERE ");
        foreach (var fItem in input.FilterFields)
        {
            if(fItem.LabelName == input.MarkField.LabelName && !string.IsNullOrEmpty(fItem.FilterValue))
            {
                //拼接sql                
                whereSql.Append(fItem.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                whereSql.Append(".");
                whereSql.Append(fItem.ColumnName);

                //判断数据类型,除时间类型用< > between...and...，其他都用in
                if (fItem.ColumnType == "DATE" || fItem.ColumnType == "TIMESTAMP")
                {
                    //前台时间控件  starttime  endtime
                    whereSql.Append(" BETWEEN TO_DATE('" + fItem.FilterValue.Split(",")[0] + "','YYYY-MM-DD HH24:MI:SS') AND TO_DATE('" + fItem.FilterValue.Split(",")[1] + "','YYYY-MM-DD HH24:MI:SS')");

                }
                else
                {
                    whereSql.Append(" in " + "(" + "'");
                    whereSql.Append(fItem.FilterValue.Replace(",", "','"));
                    // FilterValue是数组的情况
                    //foreach (var value in filters.FilterValue)
                    //{
                    //    wheresql.Append("'" + value + "'" + ",");
                    //}
                    //wheresql.Remove(-1, 1);
                    whereSql.Append("')");
                }
                whereSql.Append(" and ");
            }
        }
        if (whereSql.Length == 6)
            whereSql.Clear();
        if(whereSql.Length >= 10)  //"where " …… " and "
            whereSql.Remove(whereSql.Length - 4, 4);

        sql = $"SELECT {functionStr} FROM {tableName} {tableRename} {whereSql} {groupBy}";
        if (!functionStr.Contains('.'))
        {
            sql = dbEngineServices.showDefaultSql(functionStr, dataSource.SourceType);
        }
        var result = await dataSourceService.testDB(new DataCollectDBTest
        {
            SourceCode = dataSource.SourceCode,
            SourceType = dataSource.SourceType,
            SourceConnect = dataSource.SourceConnect,
            DynSql = sql,
            SearchAll = true
        });
        return ("OK", result.Item1.Select().Select(x => x[0].ToString()).ToList());
    }

    private (string,string) CalculatorFunction(string functionStr, string calculatorFunction, string sourceType, string groupBy)
    {
        int dotIndex = 0;
        int beginIndex = 0;
        List<string> list = new();
        string nextField;
        while(dotIndex != -1)
        {
            beginIndex = dotIndex;
            on: dotIndex = functionStr.IndexOf(',', dotIndex+1);
            if (dotIndex == -1)
            { list.Add(functionStr.Substring(beginIndex)); break; }

            // 这里判断是否当前逗号在括号内部
            nextField = functionStr.Substring(beginIndex, dotIndex - beginIndex);

            if(nextField.IndexOf('(') != -1)
            {
                string[] arr = new string[] {"(",")" };
                int leftBracket = Regex.Matches(nextField, "[(]").Count;
                int rightBracket = Regex.Matches(nextField, "[)]").Count;
                if (leftBracket != rightBracket)
                    goto on;
            }
            list.Add(nextField);
            dotIndex++;
        }
        // 开始组成新的functionStr 和 groupBy 
        if(list.Count == 1)
        {
            if (string.IsNullOrEmpty(calculatorFunction) )
            { 
                if(!dbEngineServices.checkAggregate(list[0]))
                    groupBy = string.Concat(" GROUP BY ", functionStr); 
                functionStr = list[0]; 
            }
            else
                functionStr = dbEngineServices.showFunction(calculatorFunction, sourceType).Replace("{columnName}", functionStr);
        }
        else
        {
            groupBy = " group by ";
            functionStr = "";
            for (int i = list.Count-1; i >= 0; i--)
            {
                if(i == 0 && !string.IsNullOrEmpty(calculatorFunction) )
                {
                    functionStr = string.Concat(functionStr, dbEngineServices.showFunction(calculatorFunction, sourceType).Replace("{columnName}", list[i]));
                    functionStr = string.Concat(functionStr, ",");
                }
                else
                {
                    functionStr = string.Concat(functionStr,list[i]);
                    functionStr = string.Concat(functionStr, ",");
                }

                if (!dbEngineServices.checkAggregate(list[i]))
                {
                    groupBy = string.Concat(groupBy, list[i]);
                    groupBy = string.Concat(groupBy, ",");
                }
            }
            functionStr = functionStr.Substring(0, functionStr.Length - 1);
            groupBy = groupBy.Substring(0, groupBy.Length - 1);
        }
        return (functionStr, groupBy);
    }
}
