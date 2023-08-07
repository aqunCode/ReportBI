using Bi.Core.Extensions;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using SqlSugar;
using System.Text;

namespace Bi.Services.Service;

public class AggregationServices : IAggregationServices
{
    /// <summary>
    /// 数据库sql语句引擎
    /// </summary>
    public IDbEngineServices dbEngineServices;
    /// <summary>
    /// 自定义语法分析服务
    /// </summary>
    public IBiCustomerFieldServices syntaxServices;
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    public AggregationServices(IDbEngineServices dbEngineServices
                                , ISqlSugarClient _sqlSugarClient
                                , IBiCustomerFieldServices syntaxServices)
    {
        this.dbEngineServices = dbEngineServices;
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.syntaxServices = syntaxServices;
    }

    public (string, string) AnalysisAggregate(BIWorkbookInput input)
    {
        StringBuilder sb = new(" GROUP BY ");

        // 分析行列数据
        foreach (var item in input.CalcItems)
        {
            //判断是否是聚合函数
            if (!dbEngineServices.checkAggregate(item.CalculatorFunction))
            {
                if (item.ColumnType != "2")
                {
                    sb.Append(item.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                    sb.Append('.');
                    sb.Append(item.ColumnName);
                    sb.Append(',');
                }
                else
                {

                    var res = syntaxServices.syntaxRules(new BiCustomerFieldInput
                    {
                        FieldFunction = item.CalculatorFunction,
                        DatasetId = input.DatasetId,
                        FieldCode = item.ColumnName
                    }).GetAwaiter().GetResult();
                    sb.Append(res.Item2);
                    sb.Append(",");
                }

            }

        }

        // 分析mark标记字段
        if (input.MarkItems != null)
        {
            foreach (var item in input.MarkItems)
            {
                if (!dbEngineServices.checkAggregate(item.CalculatorFunction))
                {
                    if (item.ColumnType != "2")
                    {
                        sb.Append(item.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                        sb.Append('.');
                        sb.Append(item.ColumnName);
                        sb.Append(',');
                    }
                    else
                    {
                        var res = syntaxServices.syntaxRules(new BiCustomerFieldInput
                        {
                            FieldFunction = item.CalculatorFunction,
                            DatasetId = input.DatasetId,
                            FieldCode = item.ColumnName
                        }).GetAwaiter().GetResult();
                        sb.Append(res.Item2);
                        sb.Append(",");
                    }

                }
                
            }
        }

        if (sb[sb.Length - 1] == ',')
        {
            return ("OK", sb.Remove(sb.Length - 1).ToString());
        }
        else
        {
            return ("OK", "");
        }
    }

    public async Task<(string, string)> AnalysisCalcSql(BIWorkbookInput input)
    {
        // 这个字符串不要轻易修改，外部有判断字符长度
        StringBuilder indicatorSql = new StringBuilder(" SELECT ");
        StringBuilder sb = new();
        string fieldStr = "";

        var dataset = (await repository.Queryable<BiDataset>().Where(x => x.Id == input.DatasetId).ToListAsync()).FirstOrDefault();
        var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataset.SourceCode && x.DeleteFlag == 0).ToListAsync()).FirstOrDefault();

        foreach (var calcField in input.CalcItems)
        {
            sb.Clear();
            fieldStr = "{columnName}";
            // 1 代表的是原始表字段的行列转换，2代表的是自定义字段 default 代表的是原始表字段
            switch (calcField.ColumnType)
            {
                case "1":
                    BiCustomerField field = (await repository.Queryable<BiCustomerField>().Where(x => x.Id == calcField.NodeId).ToListAsync()).FirstOrDefault();

                    sb.Append(field.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                    sb.Append('.');
                    sb.Append(field.FieldCode);
                    break;
                case "2":
                    // 这是自定义字段的解析函数  cusField.FieldFunction, dataSource.sourceType, calcField.ColumnName
                    var cusField = await repository.Queryable<BiCustomerField>().FirstAsync(x => x.Id == calcField.NodeId);
                    var res = await syntaxServices.syntaxRules(new BiCustomerFieldInput
                    {
                        FieldCode = calcField.ColumnName,
                        DatasetId = input.DatasetId,
                        FieldFunction = cusField.FieldFunction
                    });
                    var index = res.Item1.IndexOf("ERROR");
                    if (res.Item1.IndexOf("ERROR") == 0)
                        return res;

                    if (res.Item1.Contains("::"))
                    {
                        var lastIndex = res.Item1.LastIndexOf("::");
                        sb.Append(res.Item1.Substring(lastIndex + 2));
                        indicatorSql.Insert(0, res.Item1.Substring(0, lastIndex + 2));
                    }
                    else
                    {
                        sb.Append(res.Item1);
                    }
                    break;
                default:
                    // 这代表这原始表字段

                    sb.Append(calcField.LabelName.Replace(".", "").Replace("(", "").Replace(")", ""));
                    sb.Append('.');
                    sb.Append(calcField.ColumnName);
                    break;
            }
            fieldStr = fieldStr.Replace("{columnName}", sb.ToString());
            // 计算拖拽之后的计算函数
            indicatorSql.Append(dbEngineServices.showFunction(calcField.CalculatorFunction, dataSource.SourceType).Replace("{columnName}", fieldStr));
            indicatorSql.Append(' ');
            indicatorSql.Append(calcField.Axis);
            indicatorSql.Append(calcField.OrderBy);
            indicatorSql.Append('@');
        }
        int remove = indicatorSql.LastIndexOf("@");
        if (remove != -1)
        {
            indicatorSql = indicatorSql.Remove(remove, 1);
        }
        return ("OK", indicatorSql.ToString());
    }
}
