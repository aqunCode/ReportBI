using Bi.Core.Caches;
using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System.Data;
using System.Text;

namespace Bi.Services.Service;

public class DataSetServices : IDataSetServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    private IDbEngineServices dbEngine;

    private ICache redisCache;

    private ILogger<DataSetServices> logger;

    private DataSourceServices dataSourceService;

    public DataSetServices(ISqlSugarClient _sqlSugarClient,
                           IDbEngineServices dbService,
                           ICache redisCache,
                           ILogger<DataSetServices> logger,
                           DataSourceServices dataSourceService)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.dbEngine = dbService;
        this.redisCache = redisCache.UseKeyPrefix("[BI]");
        this.logger = logger;
        this.dataSourceService = dataSourceService;
    }



    /// <summary>
    /// 添加新的数据集
    /// </summary>
    public async Task<double> addAsync(DataSetInput input)
    {
        var inputentitys = await repository.Queryable<BiDataset>().Where(x => x.DatasetCode == input.DatasetCode && x.DeleteFlag == "N").ToListAsync();
        if (inputentitys.Any())
            return BaseErrorCode.PleaseDoNotAddAgain;

        var entity = input.MapTo<BiDataset>();
        entity.Create(input.CurrentUser);
        entity.Enabled = input.Enabled;

        input.Id = entity.Id;
        List<BIRelation> relations = getRelations(input, 1);
        List<BiDatasetNode> nodes = getNodes(input, 1);

        List<BINodeSqlDetail> Customsqlnodes = getNodesCustomSql(input, 1);

        try
        {
            repository.Ado.BeginTran();
            // 插入数据集
            await repository.Insertable<BiDataset>(entity).ExecuteCommandAsync();
            // 插入节点数据
            await repository.Insertable<BiDatasetNode>(nodes).ExecuteCommandAsync();
            // 插入数据relation
            await repository.Insertable<BIRelation>(relations).ExecuteCommandAsync();

            //插入自定义数据集.
            if (Customsqlnodes!=null && Customsqlnodes.Any())
            {
                await repository.Insertable<BINodeSqlDetail>(Customsqlnodes).ExecuteCommandAsync();
            }
            repository.Ado.CommitTran();
            logger.LogInformation($"[{DateTime.Now}]: 插入数据集成功 ");
        }
        catch(Exception)
        {
            logger.LogInformation($"[{DateTime.Now}]: 插入数据集失败 ");
            repository.Ado.RollbackTran();
            throw;
        }
        try
        {

        }
        catch (Exception)
        {
            logger.LogInformation($"[{DateTime.Now}]: redis缓存数据集删除失败 ");
        }
        return BaseErrorCode.Successful;
    }

    

    /// <summary>
    /// 根据ID删除数据集
    /// </summary>
    public async Task<double> deleteAsync(DataSetInput input)
    {
        var set = input.MapTo<BiDataset>();
        repository.Tracking(set);
        set.DeleteFlag = "Y";
        set.Modify(input.Id,input.CurrentUser);

        // 获取节点
        List<BiDatasetNode> nodes = await repository.Queryable<BiDatasetNode>().Where(x => x.DatasetCode == input.Id).ToListAsync();
        nodes.ForEach(x => { x.DeleteFlag = "Y";x.Modify(x.Id, input.CurrentUser); });
        // 获取边
        List<BIRelation> relations = await repository.Queryable<BIRelation>().Where(x => x.DatasetCode == input.Id).ToListAsync();
        relations.ForEach(x => { x.DeleteFlag = "Y";x.Modify(x.Id,input.CurrentUser); } );

        
        try
        {
            repository.Ado.BeginTran();
            // 删除数据集
            await repository.Updateable<BiDataset>(set).ExecuteCommandAsync();
            // 删除数据集节点信息
            await repository.Updateable<BiDatasetNode>(nodes).ExecuteCommandAsync();
            // 删除数据集关联信息
            await repository.Updateable<BIRelation>(relations).ExecuteCommandAsync();
            repository.Ado.CommitTran();
            return BaseErrorCode.Successful;
        }
        catch (Exception)
        {
            repository.Ado.RollbackTran();
            throw;
        }


    }

    /// <summary>
    /// 更新数据集
    /// </summary>
    public async Task<double> ModifyAsync(DataSetInput input)
    {
        BiDataset setTmp = (await repository.Queryable<BiDataset>().Where(x => x.Id == input.Id).ToListAsync()).FirstOrDefault();
        if (setTmp == null)
        {
            return BaseErrorCode.InvalidEncode;
        }

        BiDataset set = new();
        repository.Tracking(set);
        input.MapTo<DataSetInput,BiDataset>(set);
        set.Modify(input.Id,input.CurrentUser);

        // 查询旧节点
        List<BiDatasetNode> oldnodes = await repository.Queryable<BiDatasetNode>().Where(x => x.DatasetCode == input.Id).ToListAsync();
        // 新建node节点
        List<BiDatasetNode> nodes = getNodes(input, 1);
        // 将旧节点id赋值给新建的节点
        foreach (BiDatasetNode node in nodes)
        {
            foreach (BiDatasetNode oldnode in oldnodes)
            {
                if (node.NodeId.Contains(oldnode.NodeId))
                    node.Id = oldnode.Id;
            }
        }
        

        List<BIRelation> relations = getRelations(input, 1);
        #region 自定义字段检查删除
        // 修改后的被删除的节点是否有自定义字段？
        // 1.数据集下已创建的字段
        List<BiCustomerField> customs = await repository.Queryable<BiCustomerField>().Where(x => x.DatasetId == input.Id && x.DeleteFlag=="N").ToListAsync();
        // 2.遍历自定义字段中的表名称，查询节点中是否存在该表名称
        for(int i = 0; i< customs.Count; i++)
        {
            BiCustomerField custom = customs[i];
            int deletecus = 0;
            for(int j = 0; j < nodes.Count; j++)
            {
                BiDatasetNode node = nodes[j];
                if (custom.LabelName == node.NodeLabel)
                    deletecus++;
                //deletecus.Add((await repository.Queryable<BiCustomerField>().Where(x => x.DatasetId == input.Id && x.LabelName == node.NodeLabel && x.DeleteFlag == "N").ToListAsync()).FirstOrDefault());                              
            }
            if ( deletecus==0)
                custom.DeleteFlag = "Y";
        }
        #endregion

        #region 行列、筛选器、标记字段检查删除
        // 是否需要更改数据集后，被删除的节点相关的子表字段也删除？
        // 不处理的好处是，误删节点后再次添加，图表恢复原样
        #endregion

        try
        {
            repository.Ado.BeginTran();
            // 更新数据集
            await repository.Updateable(set).ExecuteCommandAsync();
            // 此处清空追踪，不然后续更新语句会报错  Non-static method requires a target
            repository.TempItems.Clear();
            // 删除原有节点
            await repository.Deleteable<BiDatasetNode>().Where(x => x.DatasetCode == input.Id ).ExecuteCommandAsync();
            // 删除原有relation
            await repository.Deleteable<BIRelation>().Where(x => x.DatasetCode == input.Id ).ExecuteCommandAsync();
            // 删除自定义字段（被删的节点）
            await repository.Updateable<BiCustomerField>(customs).ExecuteCommandAsync();
            // 添加新的节点信息
            await repository.Insertable<BiDatasetNode>(nodes).ExecuteCommandAsync();
            // 添加新的数据集关联信息
            await repository.Insertable<BIRelation>(relations).ExecuteCommandAsync();

            repository.Ado.CommitTran();
            return BaseErrorCode.Successful;
        }
        catch (Exception)
        {
            repository.Ado.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 分页获取所有数据集
    /// </summary>
    public async Task<PageEntity<IEnumerable<BiDataset>>> getPagelist(PageEntity<DataSetInput> inputs)
    {
        //分页查询
        RefAsync<int> total = 0;
        var input = inputs.Data;
        var data = await repository.Queryable<BiDataset>()
            .WhereIF(
                input.DatasetCode.IsNotNullOrEmpty(),
                x => x.DatasetCode.Contains(input.DatasetCode))
            .WhereIF(
                !input.DatasetName.IsNullOrEmpty(),
                x => x.DatasetName.Contains(input.DatasetName))
            .WhereIF(
                true,
                x => x.DeleteFlag == "N" )
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);

        return new PageEntity<IEnumerable<BiDataset>>
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
    /// 获取所有的数据集下拉框
    /// </summary>
    public async Task<IEnumerable<BiDataset>> getSelectlist()
    {
        var data = await repository.Queryable<BiDataset>().Where(x=> x.DeleteFlag == "N").ToListAsync();

        List<BiDataset> list = new List<BiDataset>();
        foreach (var dataset in data)
        {
            list.Add(new BiDataset
            {
                Id = dataset.Id,
                DatasetCode = dataset.DatasetCode,
                DatasetName = dataset.DatasetName
            });

        }
        return list;
    }
    /// <summary>
    /// 获取db数据源的所有用户信息
    /// </summary>
    public async Task<(string, IEnumerable<string>)> getUserlist(TableInput input)
    {
        var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == input.SourceCode && x.DeleteFlag == 0 && x.Enabled == 1).ToListAsync()).FirstOrDefault();
        if (dataSource == null)
            return ("数据源不存在或者禁用", null);

        string sql = dbEngine.showUsers(dataSource.SourceType);

        var result = await dataSourceService.testDB(new DataCollectDBTest
        {
            SourceCode = dataSource.SourceCode,
            SourceType = dataSource.SourceType,
            SourceConnect = dataSource.SourceConnect,
            DynSql = sql,
            SearchAll = true
        });
        IEnumerable<string> enums = result.Item1.Select().Select(x => x[0].ToString());

        return ("OK", enums);
    }
    /// <summary>
    /// 获取当前DB用户下的所有表
    /// </summary>
    public async Task<(string, IEnumerable<string>)> getTablelist(TableInput input)
    {
        var dataSource = (await repository.Queryable<DataSource>().Where(x=>x.SourceCode == input.SourceCode && x.DeleteFlag == 0 && x.Enabled == 1).ToListAsync()).FirstOrDefault();
        if(dataSource == null)
            return ("数据源不存在或者禁用", null);

        string sql = dbEngine.showTables(dataSource.SourceType, input.User);

        var result = await dataSourceService.testDB(new DataCollectDBTest
        {
            SourceCode = dataSource.SourceCode,
            SourceType = dataSource.SourceType,
            SourceConnect = dataSource.SourceConnect,
            DynSql = sql,
            SearchAll = true
        });
        IEnumerable<string> enums = result.Item1.Select().Select(x=> string.Concat(x[0].ToString(), ".", x[1].ToString()));

        return ("OK", enums);
    }
    /// <summary>
    /// 获取当前DB用户下的表的所有字段
    /// </summary>
    public async Task<(string, IEnumerable<ColumnInfo>)> getColumnlist(TableInput input)
    {
        var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == input.SourceCode && x.DeleteFlag == 0 && x.Enabled == 1).ToListAsync()).FirstOrDefault();
        if (dataSource == null)
            return ("数据源不存在或者禁用", null);

        List<ColumnInfo> columnInfos = new();

        var sql = string.Empty;

        switch (input.Type?.ToUpper())
        {
            case "TABLE":
                sql = dbEngine.showColumns(dataSource.SourceType, input.TableName.Split('.')[1].Trim(), input.TableName.Split('.')[0].Trim());
                break;
            case "SQL":
                var tables = dbEngine.getTablesName(input.TableName);
                sql = dbEngine.showColumns(dataSource.SourceType, tables);
                break;
            default:
                sql = dbEngine.showColumns(dataSource.SourceType, input.TableName.Split('.')[1].Trim(), input.TableName.Split('.')[0].Trim());
                break;
        }

        var result = await dataSourceService.testDB(new DataCollectDBTest
        {
            SourceCode = dataSource.SourceCode,
            SourceType = dataSource.SourceType,
            SourceConnect = dataSource.SourceConnect,
            DynSql = sql,
            SearchAll = true
        });
        var dt = result.Item1;
        if (dataSource.SourceType == "Spark")
        {
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                columnInfos.Add(new ColumnInfo
                {
                    ColumnType = "VARCHAR2",
                    ColumnComment = dt.Rows[i][0].ToString(),
                    ColumnName = dt.Rows[i][0].ToString()
                });
                
            }
        }
        else
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                columnInfos.Add(new ColumnInfo
                {
                    ColumnType = dt.Rows[i][3].ToString(),
                    ColumnComment = dt.Rows[i][2].ToString(),
                    ColumnName = dt.Rows[i][1].ToString()
                });
            }
        }
        return ("OK", columnInfos);
    }

    private List<BiDatasetNode> getNodes(DataSetInput input, int type)
    {
        List<BiDatasetNode> list = new();
        // 解析Content
        JToken content = JToken.Parse(input.Content);
        // 获取node节点
        JArray nodes = content.SelectToken("nodes")?.ToObject<JArray>();

        var conditionStr = string.Empty;

        if(nodes != null && nodes.Any())
        {
            foreach(var node in nodes)
            {
                var condition = node.SelectToken("condition")?.ToObject<JArray>();
                if(condition != null && condition.Any())
                {
                    conditionStr = getConditionStr(condition);
                }       

                BiDatasetNode biDatasetNode = new BiDatasetNode
                {
                    DatasetCode = input.Id,
                    NodeId = node.SelectToken("id").ToString(),
                    NodeLabel = node.SelectToken("label").ToString(),
                    SourceCode = input.SourceCode,
                    TableName = node.SelectToken("id")?.ToString().Split(':')[1],
                    TopLevel = node.SelectToken("depth").ToString(),
                    Condition = conditionStr,
                    Opt1 = node.SelectToken("isCustomSql").ToInt()==1?"SQL":"TABLE"
                    
                };
                if (type == 1)
                    biDatasetNode.Create(input.CurrentUser);

                list.Add(biDatasetNode);
            }
        }

        return list;
    }

    /// <summary>
    /// 解析自定义SQL
    /// </summary>
    /// <param name="input"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private List<BINodeSqlDetail> getNodesCustomSql(DataSetInput input, int type)
    {
        List<BINodeSqlDetail> list = new();
        // 解析Content
        JToken content = JToken.Parse(input.Content);
        // 获取node节点
        JArray nodes = content.SelectToken("nodes")?.ToObject<JArray>();
        // 获取自定义参数列表
        var paramslist = content.SelectToken("customParamsList")?.ToObject<JArray>();


        if (nodes != null && nodes.Any())
        {
            foreach (var node in nodes)
            {
                if (node.SelectToken("isCustomSql")?.ToString() == "1")
                {
                    var sql = new StringBuilder(node.SelectToken("customsql")?.ToString());

                    if (paramslist != null && paramslist.Any())
                    {
                        foreach (var param in paramslist)
                        {
                            sql.Replace($"@{param.SelectToken("paramName")?.ToString()}", $"'{param.SelectToken("sampleItem")?.ToString()}'");
                        }
                    }

                    BINodeSqlDetail biNodeSqlDetail = new BINodeSqlDetail
                    {
                        DataSetCode = input.Id,
                        DataSetNodeId = node.SelectToken("label").ToString(),
                        CustomizationSql = node.SelectToken("customsql")?.ToString(),
                        Alias = node.SelectToken("label").ToString(),
                        Parameter = sql.ToString(),

                    };
                    if (type == 1)
                        biNodeSqlDetail.Create(input.CurrentUser);

                    list.Add(biNodeSqlDetail);
                }

               
            }
        }

        return list;

    }

    /// <summary>
    /// 获取条件组合
    /// </summary>
    /// <param name="jArray"></param>
    /// <returns></returns>
    private string getConditionStr(JArray jArray)
    {
        string conditionString = string.Empty;

        foreach (var node in jArray)
        {
            string operatorv = node.SelectToken("operator")?.ToString();
            string columnname = node.SelectToken("columnname")?.ToString();
            string value = node.SelectToken("value")?.ToString();

            // In , > , < , = , >= , <= , <=(day) ,>=(datetime) , <=(datetime)
            switch (operatorv)
            {
                case "In":
                    conditionString += $" AND {columnname} In [{value}] ";
                    break;
                case ">":
                case "<":
                case "=":
                case ">=":
                case "<=":
                    conditionString += $" AND {columnname} {operatorv} {value} ";
                    break;
                case "<=(day)":
                    conditionString += $" AND {columnname} <= SYSDATE-{value} ";
                    break;
                case ">=(day)":
                    conditionString += $" AND {columnname} >= SYSDATE-{value} ";
                    break;
                case ">=(datetime)":
                    conditionString += $" AND {columnname} >= TO_DATE({value},'YYYY-MM-DD HH24:MI:SS')";
                    break;
                case "<=(datetime)":
                    conditionString += $" AND {columnname} <= TO_DATE({value},'YYYY-MM-DD HH24:MI:SS')";
                    break;
                default:
                    conditionString += " ";
                    break;
            }
        }

        return conditionString.TrimStart(" AND");
    }

    public List<BIRelation> getRelations(DataSetInput input,int type)
    {
        List<BIRelation> list = new();
        // 解析Content
        JToken content = JToken.Parse(input.Content);
        // 获取node节点
        JArray nodes = content.SelectToken("nodes")?.ToObject<JArray>();
        // 获取节点relation
        JArray edges = content.SelectToken("edges")?.ToObject<JArray>();
        if (edges != null && edges.Any())
        {
            foreach (var edge in edges)
            {
                var relation = new BIRelation
                {
                    DatasetCode = input.Id,
                    SourceId = edge.SelectToken("source")?.ToString(),
                    TargetId = edge.SelectToken("target")?.ToString(),
                    TopLevel = (nodes.Where(x => x.SelectToken("id").ToString() == edge.SelectToken("source").ToString()).FirstOrDefault()).SelectToken("depth").ToString(),
                    JoinRelational = edge.SelectToken("relations")?.ToString(),
                    IncidenceRelation = edge.SelectToken("incidenceRelation")?.ToString()
                };
                if (type == 1)
                    relation.Create(input.CurrentUser);
                else
                    relation.Modify(input.Id,input.CurrentUser);
                list.Add(relation);
            }
        }
        return list;
    }

    
}

