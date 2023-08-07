using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Bi.Services.Service;
/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2022/12/28 16:54:37
/// 版本：1.0
/// </summary>
public class BiCustomerFieldServices : IBiCustomerFieldServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;
    /// <summary>
    /// 用于获取所有的表字段属性
    /// </summary>
    private IBIWorkbookDataServices biWorkbookDataServices;
    /// <summary>
    /// 用来获取注入的
    /// </summary>
    private readonly IServiceProvider serviceProvider;

    private IServiceScopeFactory scopeFactory;

    public BiCustomerFieldServices(   ISqlSugarClient _sqlSugarClient
                                    , IBIWorkbookDataServices biWorkbookDataServices
                                    , IServiceProvider serviceProvider
                                    , IServiceScopeFactory scopeFactory)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.biWorkbookDataServices = biWorkbookDataServices;
        this.serviceProvider = serviceProvider;
        this.scopeFactory = scopeFactory;
    }



    /// <summary>
    /// 添加新的BiCustomerField
    /// </summary>
    public async Task<double> addAsync(BiCustomerFieldInput input)
    {
        var inputentitys = await repository.Queryable<BiCustomerField>().Where(x =>  x.DatasetId == input.DatasetId && x.FieldCode == input.FieldCode && x.DeleteFlag == "N").ToListAsync();
        if (inputentitys.Any())
            return BaseErrorCode.PleaseDoNotAddAgain;        
        var entity = input.MapTo<BiCustomerField>();
        if (input.Remark == "2")
        {
            var sqlanddatatype = await syntaxRules(input);
            entity.DataType = sqlanddatatype.Item2;
        }        
        entity.Create(input.CurrentUser);
        entity.Enabled = input.Enabled;
        await repository.Insertable<BiCustomerField>(entity).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 根据ID删除BiCustomerField
    /// </summary>
    public async Task<double> deleteAsync(BiCustomerFieldInput input)
    {
        var set = input.MapTo<BiCustomerField>();
        repository.Tracking(set);
        set.DeleteFlag = "Y";
        set.Modify(input.Id,input.CurrentUser);
        await repository.Updateable<BiCustomerField>(set).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 更新BiCustomerField
    /// </summary>
    public async Task<double> ModifyAsync(BiCustomerFieldInput input)
    {
        BiCustomerField set = new();
        repository.Tracking(set);
        input.MapTo<BiCustomerFieldInput,BiCustomerField>(set);
        if (input.Remark == "2")
        {
            var sqlanddatatype = await syntaxRules(input);
            set.DataType = sqlanddatatype.Item2;
        }
        set.Modify(input.Id,input.CurrentUser);
        await repository.Updateable<BiCustomerField>(set).ExecuteCommandAsync();
        return BaseErrorCode.Successful;
    }

    /// <summary>
    /// 分页获取所有BiCustomerField
    /// </summary>
    public async Task<PageEntity<IEnumerable<BiCustomerField>>> getPagelist(PageEntity<BiCustomerFieldInput> inputs)
    {
        //分页查询
        RefAsync<int> total = 0;
        var input = inputs.Data;
        var data = await repository.Queryable<BiCustomerField>()
             .WhereIF(
                 true,
                 x => x.DeleteFlag == "N")
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);

        return new PageEntity<IEnumerable<BiCustomerField>>
        {
            PageIndex = inputs.PageIndex,
            Ascending = inputs.Ascending,
            PageSize = inputs.PageSize,
            OrderField = inputs.OrderField,
            Total = (long)total,
            Data = data
        };
    }

    public async Task<(double, BiCustomerField)> getEntity(BiCustomerFieldInput input)
    {
        var data = await repository.Queryable<BiCustomerField>()
            .WhereIF(
                !string.IsNullOrEmpty(input.Id),
                x=>x.Id.Contains(input.Id)
                )
            .WhereIF(
                 true,
                 x => x.DeleteFlag == "N")
            .Take(1)
            .ToListAsync();
        var res = data.FirstOrDefault();
        if (res == null)
            return (BaseErrorCode.Fail,null);
        return (BaseErrorCode.Successful, data.FirstOrDefault());
    }


    public async Task<(string, string)> syntaxRules(BiCustomerFieldInput input)
    {
        string fieldFunction = input.FieldFunction; // MAX(INT([DDAY(LEDRPT.RPT_UNIT_TRACKOUT_DETAIL)]))
        string datasetId = input.DatasetId; // 2B1ACE9B5E7147DDABBC0482A948E55F

        //增加自定义函数  FieldFunction 为空时，根据对应的FieldCode 找到对应的FieldFunction，并赋值给fieldFunction
        if (string.IsNullOrWhiteSpace(fieldFunction))
        {
            fieldFunction = (await repository.Queryable<BiCustomerField>().Where(x => x.DatasetId == input.DatasetId && x.FieldCode == input.FieldCode).ToListAsync()).FirstOrDefault().FieldFunction;
        }

        // 优先转化语法中的字段信息
        var dataResult = await biWorkbookDataServices.getColumninfo(input.DatasetId);

        var columnInfos = dataResult.Item2;
        int index = 0;
        int beginIndex = 0;
        string fieldStr;
        Dictionary<string, SyntaxDataType> dic = new();
        while (index < fieldFunction.Length)
        {
            index = fieldFunction.IndexOf("[");
            if (index == -1)
                break;
            beginIndex = index;
            index = fieldFunction.IndexOf("]", index + 1);
            if(index == -1)
            {
                return ($"ERROR 列名信息不完整 索引位置：【{beginIndex}】！", "ERROR");
            }
            fieldStr = fieldFunction.Substring(beginIndex + 1, index - beginIndex - 2);
            var lableName = fieldStr.Substring(fieldStr.IndexOf('(')+1);
            var columnName = fieldStr.Substring(0,fieldStr.IndexOf('('));
            var node = await repository.Queryable<BiDatasetNode>().FirstAsync(x => x.DatasetCode == datasetId && x.NodeLabel == lableName);
            if (node!=null)
            {
                var replaceStr = $" {lableName.Trim().Replace(".", "").Replace("(", "").Replace(")", "")}.{columnName} ";
                fieldFunction = fieldFunction.Replace($"[{fieldStr})]", replaceStr);
                var column = columnInfos.First(x => x.LabelName == lableName && x.ColumnName == columnName);
                if(column == null)
                    return ($"ERROR 列名【{columnName}】不存在！", "ERROR");
                if (column.ColumnType == "2")
                    return ( $"ERROR 禁止计算字段嵌套【{columnName}】！", "ERROR");
                if(!dic.ContainsKey(replaceStr.Trim()))
                    dic.Add(replaceStr.Trim(), (SyntaxDataType)Enum.Parse(typeof(SyntaxDataType),column.DataType));
            }
            else
            {
                return ($"ERROR 当前数据集中未找到节点(表名)【{lableName}】", "ERROR");
            }
        }

        // 获取数据源类型
        var set = await repository.Queryable<BiDataset>().SingleAsync(x => x.Id == datasetId);
        var source = await repository.Queryable<DataSource>().FirstAsync(x => x.SourceCode == set.SourceCode && x.DeleteFlag == 0);
        (string, string) res;
        using (var scope = scopeFactory.CreateScope())
        {
            var syntaxServices = scope.ServiceProvider.GetRequiredService<ISyntaxServices>();
            res = syntaxServices.syntaxFuction(fieldFunction, source.SourceType, input.FieldCode, dic);
        }
        return res;

    }
}

