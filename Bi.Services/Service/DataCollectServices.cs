using Bi.Core.Models;
using Bi.Core.Extensions;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using MagicOnion;
using Bi.Entities.Entity;

namespace Bi.Services.Service;

//名称空间之中引用的名称空间代表优先使用依赖
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Bi.Core.Const;

public class DataCollectServices : IDataCollectServices {

    private readonly ILogger<DataCollectServices> logger;

    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;
    /// <summary>
    /// datasource 服务接口
    /// </summary>
    private readonly IDataSourceServices dataSourceService;

    public DataCollectServices(ISqlSugarClient _sqlSugarClient, IDataSourceServices service, ILogger<DataCollectServices> logger) {

        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.dataSourceService = service;
        this.logger = logger;
    }

    public async UnaryResult<double> addAsync(DataCollectInput input) {

        var inputentitys = await repository.Queryable<DataCollect>().Where(x => x.SetCode == input.SetCode).ToListAsync();
        if(inputentitys.Any())
            throw new Exception("数据集编码重复！");

        foreach(DataCollectItem item in input.DataSetParamDtoList) {
            item.SetCode = input.SetCode;
            var entityItem = item.MapTo<DataCollectItem>();
            entityItem.Create(input.CurrentUser);
            repository.Insertable(entityItem).ExecuteCommand();
        }
        var entity = input.MapTo<DataCollect>();
        entity.Create(input.CurrentUser);
        repository.Insertable(entity).ExecuteCommand();
        return BaseErrorCode.Successful;
    }
    public async UnaryResult<double> deleteAsync(DataCollectDelete input) {
        int i = -1;
        foreach(String setCode in input.SetCode) {
            var inputentitys = (await repository.Queryable<DataCollect>().Where(x => x.SetCode == setCode && x.DeleteFlag == 0).ToListAsync());
            if(!inputentitys.Any()) {
                continue;
            } else {
                var entity = inputentitys.First();
                repository.Tracking(entity);
                entity.DeleteFlag = 1;
                entity.Modify(inputentitys.First().Id, input.CurrentUser);
                await repository.Updateable(entity).ExecuteCommandAsync();
                i++;
            }
        }
        return i + 1;
    }
    public async UnaryResult<double> ModifyAsync(DataCollectInput input) {
        var list = await repository.Queryable<DataCollect>().Where(x => x.SetCode == input.SetCode).ToListAsync();
        if(!list.Any()) {
            return BaseErrorCode.InvalidEncode;
        }
        var inputentity = list.First();
        //修改主表
        DataCollect modifyRecord = input.MapTo<DataCollect>();
        modifyRecord.Modify(inputentity.Id, input.CurrentUser);
        repository.Updateable(modifyRecord).ExecuteCommand();
        //删除Item 表并重新添加
        repository.Deleteable<DataCollectItem>(x => x.SetCode == input.SetCode).ExecuteCommand();
        foreach (var item in input.DataSetParamDtoList) {
            item.SetCode = input.SetCode;
            item.Create(input.CurrentUser);
        }
        repository.Insertable(input.DataSetParamDtoList).ExecuteCommand();
        return BaseErrorCode.Successful;
    }

    public async Task<IEnumerable<DataSourceName>> getAllDataSource(DataSourceName input) {
        IEnumerable<DataSource> list;
        var dt = repository.Ado.GetDataTable(@"
        SELECT sdd2.DETAILCODE FROM GITEA.SYS_DATAITEM sdd
        LEFT JOIN GITEA.SYS_DATAITEM_DETAIL sdd2 ON sdd.ID = sdd2.ITEMID
        WHERE sdd.ITEMCODE = 'dataSource'");
        var arr = dt.Select().Select(x => x["DETAILCODE"].ToString()).ToArray();
        //String[] arr = new string[] { "MySql", "Oracle", "PostgreSql", "SqlServer", "Sqlite" };
        if(input.SourceType == "http")
            list = await repository.Queryable<DataSource>().Where(x => x.SourceType == input.SourceType && x.DeleteFlag == 0).ToListAsync();
        else if(input.SourceType == "sql")
            list = await repository.Queryable<DataSource>().Where(x => arr.Contains(x.SourceType) && x.DeleteFlag == 0).ToListAsync();
        else
            list = await repository.Queryable<DataSource>().Where(x => x.DeleteFlag == 0).ToListAsync();
        List<DataSourceName> results = new List<DataSourceName>();
        foreach(var dataSource in list) {
            results.Add(new DataSourceName {
                SourceCode = dataSource.SourceCode,
                SourceName = dataSource.SourceName
            });
        }
        return results;
    }

    public async Task<DataCollectResponse> getDetailBysetCode(DataCollectInput input) {
        DataCollectResponse result = (await repository.Queryable<DataCollect>().Where(x => x.SetCode == input.SetCode).ToListAsync()).FirstOrDefault()
                                                      .MapTo<DataCollectResponse>();

        List<String> list = getSetParamList(result.CaseResult,result.SetType,result.SetDesc);
        result.SetParamList = list;

        #region
        /*dynamic DynamicObject = JsonConvert.DeserializeObject<dynamic>(result.caseResult);
        foreach (var item in DynamicObject) {
            item.Name.ToString();
        }
        JToken*/
        /*if('[' == result.caseResult.First())
            result.setParamList = JsonConvert.DeserializeObject<List<Dictionary<String, String>>>(result.caseResult)[0]
                                         .Keys.ToList();
        else
            result.setParamList = JsonConvert.DeserializeObject<Dictionary<String, String>>(result.caseResult)
                                         .Keys.ToList();
        */
        #endregion
        result.DataSetParamDtoList = await repository.Queryable<DataCollectItem>().Where(x => x.SetCode == input.SetCode && x.DeleteFlag == 0).ToListAsync();

        return result;
    }

    

    private Boolean integerCheck(string value) {
        return Regex.IsMatch(value, @"^[+-]?\d*$");
    }

    public async Task<PageEntity<IEnumerable<DataCollect>>> getEntityListAsync(PageEntity<DataCollectInput> inputs) {
        //分页查询
        RefAsync<int> total = 0;
        var input = inputs.Data;
        var data = await repository.Queryable<DataCollect>()
            .WhereIF(
                !input.SetCode.IsNullOrEmpty(),
                x => x.SetCode == input.SetCode)
            .WhereIF(
                !input.SourceCode.IsNullOrEmpty(),
                x => x.SourceCode.Contains(input.SourceCode))
            .WhereIF(
                !input.SetName.IsNullOrEmpty(),
                x => x.SetName.Contains(input.SetName))
            .WhereIF(
                true,
                x => x.DeleteFlag == 0)
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);
            
        foreach(DataCollect item in data) {
            item.SourceName = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == item.SourceCode).ToListAsync()).FirstOrDefault().SourceName;
        }
        return new PageEntity<IEnumerable<DataCollect>> {
            PageIndex = inputs.PageIndex,
            Ascending = inputs.Ascending,
            PageSize = inputs.PageSize,
            OrderField = inputs.OrderField,
            Total = (long)total,
            Data = data
        };
    }


    public async Task<PageEntity<IEnumerable<object>>> getFirstValues(PageEntity<DataCollectInput> inputs)
    {
        inputs.Data.LimitStart = (inputs.PageIndex-1)*inputs.PageSize+1;
        inputs.Data.LimitEnd = 999999;
        inputs.Data.SetCode = inputs.Data.SetCode.Replace("∞", "");
        inputs.Data.DataSetParamDtoList =  await repository.Queryable<DataCollectItem>().Where(x => x.SetCode == inputs.Data.SetCode).ToListAsync();
        var result =  await testTransform(inputs.Data);
        JToken jtoken = JToken.Parse(result.Item1);
        if(jtoken.Type == JTokenType.Array)
        {
            JArray array = jtoken.ToObject<JArray>();
            var res = array.Select(x => 
                x.First().ToObject<JProperty>().Value.ToString()
            );
            if (inputs.Data.Value.IsNullOrEmpty())
            {
                inputs.Data.Value = "";
            }
            return new PageEntity<IEnumerable<object>>
            {
                Ascending = inputs.Ascending,
                OrderField = inputs.OrderField,
                PageIndex = inputs.PageIndex,
                PageSize = inputs.PageSize,
                Total = array.Count,
                Data = new JArray(
                            from x in res.Distinct()
                            where x.Contains(inputs.Data.Value.Replace("∞", ""))
                            select new JObject(
                                new JProperty("value", x)
                                )
                            ).Take(inputs.PageSize)
            };
        }
        return new PageEntity<IEnumerable<object>>
        {
            Ascending = inputs.Ascending,
            OrderField = inputs.OrderField,
            PageIndex = inputs.PageIndex,
            PageSize = inputs.PageSize,
            Total = 0,
            Data = new List<Object>(),
        }; 
        
    }
    public async Task<(string,long ,long,DataTable)> testTransform(DataCollectInput input) {
        //Boolean flag = false;
        // 获取数据集
        DateTime dt = System.DateTime.Now;
        DataCollect dataCollect = (await repository.Queryable<DataCollect>().Where(x => x.SetCode == input.SetCode && x.DeleteFlag == 0).ToListAsync()).FirstOrDefault();
        dataCollect = dataCollect ?? new DataCollect();
        if ("sql" == (input.SetType.IsNull() ? dataCollect.SetType : input.SetType)) {
            //获取数据源
            List<DataSource> dataSources = await repository.Queryable<DataSource>().Where(x => x.SourceCode == (input.SourceCode??dataCollect.SourceCode)).ToListAsync();
            DataSource dataSource = dataSources.FirstOrDefault();
            //获取参数列表
            String dynSql = replaceCustome("sql", dataSource.SourceType, input.SetCode, input.DynSentence.IsNullOrEmpty() ? dataCollect.DynSentence : input.DynSentence, input.DataSetParamDtoList);
            logger.LogInformation($"[{DateTime.Now}] setcode:[{input.SetCode}] SQL开始执行{dynSql}");
            DateTime dt2 = System.DateTime.Now;
            // 调用服务查询数据
            var tmp = await dataSourceService.testDB(new DataCollectDBTest {
                SourceCode =dataSource.SourceCode, 
                SourceType = dataSource.SourceType,
                SourceConnect = dataSource.SourceConnect,
                DynSql = dynSql,
                LimitStart = input.LimitStart,
                LimitEnd = input.LimitEnd,
                OrderArr = input.OrderArr,
                SearchAll = input.SearchAll,
                Export = input.Export,
                GroupList = input.GroupList,
                SqlFiltration = input.SqlFiltration
            });
            DateTime dt1 = System.DateTime.Now;
            double t =  dt1.Subtract(dt).TotalMilliseconds;
            logger.LogInformation($"[{DateTime.Now}]SQL执行时间:{t}");
            if (tmp.Item1 != null) {
                
                if (input.IsPreview)
                {
                    return (null, tmp.Item2, tmp.Item3, tmp.Item1);
                }
                else
                {
                    var dataStr = DataTableToJson(tmp.Item1);
                    tmp.Item1 = null;
                    GC.Collect();
                    return (dataStr, tmp.Item2, tmp.Item3, null);
                }
            }
                
        } else if("http" == (input.SetType.IsNull() ? dataCollect.SetType : input.SetType)) {
            int pageSize = input.LimitEnd - input.LimitStart;
            int pageIndex;
            if(pageSize == 0) {
                pageSize = 500;
                pageIndex = 1;
                input.LimitStart = 1;
                input.LimitEnd = 500;
            } else {
                pageSize++;
                pageIndex = input.LimitStart / pageSize + 1;
            }
            //获取数据源
            List<DataSource> dataSources = await repository.Queryable<DataSource>().Where(x => x.SourceCode == (input.SourceCode == null?dataCollect.SourceCode: input.SourceCode)).ToListAsync();
            DataSource dataSource = dataSources.FirstOrDefault();
            //获取参数列表
            String dynBody = replaceCustome("http","", input.SetCode, input.DynSentence?? dataCollect.DynSentence, input.DataSetParamDtoList);
            // 插入分页条件 dynBody 
            string pageSizeStr = "pageSize";
            string pageIndexStr = "pageIndex";
            if(dynBody.IndexOf("${pageSize}") != -1){
                int index1 = dynBody.IndexOf("${pageSize}");
                string[] arr = dynBody.Substring(0, index1).Split(',');
                pageSizeStr = arr[arr.Length - 1].Replace("\"", "").Replace(":", "");
                dynBody = dynBody.Replace("${pageSize}", pageSize.ToString());
            }
            if (dynBody.IndexOf("${pageIndex}") != -1)
            {
                int index1 = dynBody.IndexOf("${pageIndex}");
                string[] arr = dynBody.Substring(0, index1).Split(',');
                pageIndexStr = arr[arr.Length - 1].Replace("\"", "").Replace(":", "");
                dynBody = dynBody.Replace("${pageIndex}", pageIndex.ToString());
            }
            var tmp = await dataSourceService.testHttp(new DataSourceInput {
                HttpWay = dataSource.HttpWay,
                HttpHeader = dataSource.HttpHeader,
                HttpBody = dynBody,
                HttpAddress = dataSource.HttpAddress
            });
            if((int)tmp.Item2 == 200 && tmp.Item1 != null) {
                try {
                    // #region 此处进行分页处理
                    JToken jToken = JToken.Parse(tmp.Item1);
                    List<JToken> list = new List<JToken>();
                    if(jToken.Type == JTokenType.Array) {
                        for(int i = 0;i<jToken.Count() ; i++) {
                            if(i+1>= input.LimitStart && i+1 <= input.LimitEnd)
                                list.Add(jToken[i]);
                        }
                        int pageNum = jToken.Count()>0? jToken.Count() % pageSize == 0 ? jToken.Count() / pageSize : (jToken.Count() / pageSize) + 1 : 0;
                        if (input.IsPreview)
                            return (null, pageNum, jToken.Count(), toDataTable(list));
                        else
                            return (JsonConvert.SerializeObject(list), pageNum, jToken.Count(), null);
                    } else {
                        int total = 0;
                        String[] arr = dataCollect != null? dataCollect.SetDesc?.Split('.'):(input.SetDesc?.Split('.'));
                        arr = arr??new string[]{};
                        JToken lastTmp = jToken;
                        for(int i = 0; i < arr.Length; i++) {
                            if(i == arr.Length-1) {
                                lastTmp = jToken;
                            }
                            // 根据设定时的pageIndex 和 pageSize 获取查询数据总数
                            if(jToken.SelectToken(pageSizeStr) != null || jToken.SelectToken(pageIndexStr) != null)
                            {
                                JObject pageEntity =  jToken.ToObject<JObject>();
                                // 此处没办法完成学习模型，条件受限，希望下次自己可以搞
                                int level = 100;
                                foreach(var item in pageEntity)
                                {
                                    if(item.Value.Type == JTokenType.Integer)
                                    {
                                        
                                        if(item.Key != pageIndexStr && item.Key != pageSizeStr)
                                        { // 此时多半表示总数了
                                            if(item.Key.ToLower() == "total")
                                            {
                                                total = Convert.ToInt32(item.Value);
                                                level = 1;
                                                break;
                                            }else if(item.Key.ToLower() == "count")
                                            {
                                                total = Convert.ToInt32(item.Value);
                                                level = 2;
                                                break;
                                            }
                                            else if (item.Key.ToLower().IndexOf("count") != -1)
                                            {
                                                if (level > 3)
                                                {
                                                    total = Convert.ToInt32(item.Value);
                                                    level = 3;
                                                }
                                            }
                                            else if(item.Key.ToLower().IndexOf("total")!=-1)
                                            {
                                                if (level > 4)
                                                {
                                                    total = total == 0 ? Convert.ToInt32(item.Value) : total;
                                                    level = 4;
                                                }
                                            }
                                            else if (item.Key.ToLower().IndexOf("num") != -1)
                                            {
                                                if (level > 5)
                                                {
                                                    total = total == 0 ? Convert.ToInt32(item.Value) : total;
                                                    level = 5;
                                                }
                                                total = total == 0 ? Convert.ToInt32(item.Value) : total;
                                            }
                                            else
                                            {
                                                if(level > 6)
                                                {
                                                    total = total == 0 ? Convert.ToInt32(item.Value) : total;
                                                    level = 6;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if(jToken.Count() == 0 || integerCheck(arr[i]) && Convert.ToInt32(arr[i]) > jToken.Count())
                            {
                                break;
                            }
                            jToken = jToken[integerCheck(arr[i]) ? Convert.ToInt32(arr[i]) : arr[i]];
                        }
                        if(lastTmp.Type == JTokenType.Array && jToken.Type == JTokenType.Object) {
                            jToken = lastTmp;
                        }
                        if(jToken.Type == JTokenType.Array) {
                            for(int i = 0; i < jToken.Count(); i++) {
                                // if(i + 1 >= input.limitStart && i + 1 <= input.limitEnd)
                                list.Add(jToken[i]);
                            }
                            total = Math.Max(jToken.Count(), total);
                            int pageNum = total > 0 ? (total % pageSize == 0 ? total / pageSize : (total / pageSize) + 1) : 0;
                            if (input.IsPreview)
                                return (null, pageNum, total, toDataTable(list));
                            else
                                return (JsonConvert.SerializeObject(list), pageNum, total, null);
                        } else {
                            return (JsonConvert.SerializeObject(jToken), 0, 0, null);
                        }
                    }
                } catch (Exception) {}
                return (tmp.Item1, 0, 0, null);
            }
        } else {
            return (null,0,0,null);
        }
        return (null, 0, 0, null);
    }
    /// <summary>
    /// 转化json为DataTable
    /// </summary>
    public DataTable toDataTable(List<JToken> list)
    {
        DataTable table = new DataTable();
        
        if (list.Count > 0)
        {
            StringBuilder columns = new StringBuilder();

            JObject objColumns = list[0] as JObject;
            //构造表头
            foreach (JToken jkon in objColumns.AsEnumerable<JToken>())
            {
                string name = ((JProperty)(jkon)).Name;
                columns.Append(name + ",");
                table.Columns.Add(name);
            }
            //向表中添加数据
            for (int i = 0; i < list.Count; i++)
            {
                DataRow row = table.NewRow();
                JObject obj = list[i] as JObject;
                foreach (JToken jkon in obj.AsEnumerable<JToken>())
                {

                    string name = ((JProperty)(jkon)).Name;
                    string value = ((JProperty)(jkon)).Value.ToString().Replace("\"", "");
                    row[name] = value;
                }
                table.Rows.Add(row);
            }
        }
        return table;
    }

    private String replaceCustome(String type,string sourceType, string setCode, String dynSentence, List<DataCollectItem> list) {
        StringBuilder sb = new StringBuilder();
        //去掉回车换行符
        dynSentence = dynSentence.Replace("\r", " \r ").Replace("\n", " \n ");
        //添加空格避免解析出错
        dynSentence = dynSentence.Replace("(", " ( ").Replace(")", " ) ");
        // 异常情况终止循环
        int endNum = 0;
        foreach(DataCollectItem item in list) {
            // and username =  '${u}aa'
            endNum = 0;
            sb.Clear();
            sb.Append("${");
            sb.Append(item.ParamName);
            sb.Append("}");
            if(item.SetCode == null && setCode != null) {
                List<DataCollectItem> dataCollectItems = repository.Queryable<DataCollectItem>().Where(x => x.SetCode == setCode && x.ParamName == item.ParamName).ToList();
                if(!dataCollectItems.Any()) { continue; }
                item.RequiredFlag = dataCollectItems.First().RequiredFlag;
                item.ParamType = dataCollectItems.First().ParamType;
            }
            if("http" == type && item.SampleItem.IsNullOrEmpty())
            {
                if(item.RequiredFlag == 1)
                {
                    throw new Exception("当前必选栏位数据为null，请check！" + item.ParamName);
                }
                string replaceStr = sb.ToString();
                if (dynSentence.IndexOf($"\"{replaceStr}\"") != -1 && item.ParamType == "Array")
                    replaceStr = $"\"{replaceStr}\"";

                dynSentence = dynSentence.Replace(replaceStr, item.SampleItem);
            }
            if("http" != type && item.SampleItem.IsNullOrEmpty()) {
                if(item.RequiredFlag == 1) {
                    throw new Exception("当前必选栏位数据为null，请check！" + item.ParamName);
                }
                begin: int index = dynSentence.IndexOf(sb.ToString());
                if(endNum++ > 20)
                    throw new Exception("去除非必要参数超过循环上线20次，请联系IT!" + item.ParamName);
                // 配置错误，直接返回
                if(index == -1)
                    continue;
                int firstIndex = index;
                int secondIndex = index + sb.Length;
                int brackets = 0;
                int totalLength = dynSentence.Length;
                // 判断是否是字符，带引号
                if(dynSentence[secondIndex] == '\'')
                    secondIndex ++;
                else if(dynSentence[secondIndex] == '%')
                    secondIndex+=2;

                while (true) {
                    firstIndex--;
                    if(firstIndex <= 0) {
                        break;
                    }
                    if('(' == dynSentence[firstIndex]) {
                        brackets++;
                    } else if(')' == dynSentence[firstIndex]) {
                        brackets--;
                    }
                    if( 'a' == dynSentence[firstIndex] || 
                        'A' == dynSentence[firstIndex] || 
                        'o' == dynSentence[firstIndex] || 
                        'O' == dynSentence[firstIndex]) {

                        if( dynSentence.Substring(firstIndex - 1, 5).ToLower() == " and " || 
                            dynSentence.Substring(firstIndex - 1, 5).ToLower() == "'and " ||
                            dynSentence.Substring(firstIndex - 1, 5).ToLower() == " or "  ||
                            dynSentence.Substring(firstIndex - 1, 5).ToLower() == "'or ") {
                            String firstStr = dynSentence.Substring(0, firstIndex);

                            if(brackets == 0) {
                                firstStr += dynSentence.Substring(secondIndex);
                            } else {
                                for(int i = 0; i < brackets; i++) {
                                    secondIndex = dynSentence.IndexOf(')', secondIndex + 1);
                                }
                                secondIndex++;
                                firstStr += dynSentence.Substring(secondIndex);
                            }
                            dynSentence = firstStr;
                            break;
                        }
                        continue;
                    } else if('w' == dynSentence[firstIndex] || 'W' == dynSentence[firstIndex]) {
                        if(dynSentence.Substring(firstIndex - 1, 7).ToLower() == " where ") {
                            String firstStr = dynSentence.Substring(0, firstIndex);
                            if(brackets == 0) {
                                String secondStr = dynSentence.Substring(secondIndex).Trim();
                                /*if(secondIndex >= totalLength || secondStr.Trim().IsNullOrEmpty())
                                    goto w;*/
                                if(secondStr.Length > 4 && (secondStr.Substring(0, 4).ToLower() == "and " || secondStr.Substring(0, 3).ToLower() == "or ")) {
                                    firstStr += " where ";
                                    firstStr += secondStr.Substring(3);
                                } else {
                                    firstStr += secondStr;
                                }
                            } else {
                                for(int i = 0; i < brackets; i++) {
                                    secondIndex = dynSentence.IndexOf(')', secondIndex + 1);
                                }
                                secondIndex++;
                                String secondStr = dynSentence.Substring(secondIndex).Trim();
                                if(secondStr.Length > 4 && (secondStr.Substring(0, 4).ToLower() == "and " || secondStr.Substring(0, 3).ToLower() == "or ")) {
                                    firstStr += " where ";
                                    firstStr += secondStr.Substring(3);
                                } else {
                                    firstStr += secondStr;
                                }
                            }
                            dynSentence = firstStr;
                            break;
                        }
                    }
                }
                goto begin;
            }
            switch(item.ParamType ) {
                case "Array":
                    logger.LogInformation($"[{DateTime.Now}]替换 {type}  {item.ParamType}");
                    String tmp;
                    switch(type) {
                        case "http":
                            dynSentence = dynSentence.Replace(sb.ToString(), item.SampleItem?.Replace(",", "\",\"").Replace("\r", "\",\"").Replace("\n", "\",\""));
                            break;
                        case "sql":
                            if (sourceType == "Oracle")
                            {
                                tmp = item.SampleItem.Replace(",", "','").Replace("\r", "','").Replace("\n", "','");
                                int num = Regex.Matches(tmp, ",").Count;
                                logger.LogInformation($"[{DateTime.Now}]进入oracle选项数量如下{num}");
                                if (num > 998)
                                {
                                    //超过999 oracle 会报错
                                    StringBuilder with = new StringBuilder("with temp as (");
                                    String[] arr = ('\'' + tmp + '\'').Split(',');
                                    for (int i = 0; i < arr.Length; i++)
                                    {
                                        with.Append(" select ");
                                        with.Append(arr[i]);
                                        with.Append(" param from dual union all ");
                                    }
                                    with = with.Remove(with.Length - 10);
                                    with.Append(')');
                                    if (dynSentence.TrimStart().Substring(0, 4).ToLower() == "with")
                                    {
                                        dynSentence = ',' + dynSentence.Substring(4);
                                    }
                                    if (dynSentence.IndexOf('\'' + sb.ToString() + '\'') != -1)
                                    {
                                        dynSentence = with + dynSentence.Replace('\'' + sb.ToString() + '\'', " select param from temp ");
                                    }
                                    else
                                    {
                                        dynSentence = with + dynSentence.Replace(sb.ToString(), " select param from temp ");
                                    }
                                }
                                else
                                {
                                    dynSentence = dynSentence.Replace(sb.ToString(), tmp);
                                }
                            }
                            else
                            {
                                tmp = item.SampleItem.Replace(",", "','").Replace("\r", "','").Replace("\n", "','");
                                dynSentence = dynSentence.Replace(sb.ToString(), tmp);
                            }
                                break;
                        default:
                            tmp = item.SampleItem.Replace(",", "','").Replace("\r", "','").Replace("\n", "','");
                            dynSentence = dynSentence.Replace(sb.ToString(), tmp);
                            break;
                    }
                    
                    break;
                default:
                    dynSentence = dynSentence.Replace(sb.ToString(), item.SampleItem);
                    break;
            }
        }
        return dynSentence;
    }
    /// <summary>
    /// 手动将DataTable 转化为Json
    /// </summary>
    private string DataTableToJson(DataTable table)
    {
        var JsonString = new StringBuilder();
        if (table.Rows.Count > 0)
        {
            JsonString.Append("[");
            for (int i = 0; i < table.Rows.Count; i++)
            {
                JsonString.Append("{");
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    if (j < table.Columns.Count - 1)
                    {
                        JsonString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString().Replace("\"","\\\"").Replace("\n", "").Replace("\r", "") + "\",");
                    }
                    else if (j == table.Columns.Count - 1)
                    {
                        JsonString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString().Replace("\"", "\\\"").Replace("\n", "").Replace("\r", "") + "\"");
                    }
                }
                if (i == table.Rows.Count - 1)
                {
                    JsonString.Append("}");
                }
                else
                {
                    JsonString.Append("},");
                }
            }
            JsonString.Append("]");
        }
        return JsonString.ToString();
    }


    /// <summary>
    /// miniExcel 导出之 IDataReader 导出
    /// </summary>
    public async Task<(IDataReader,string)> testTransform(DataCollectReader input)
    {
        DateTime dt = System.DateTime.Now;
        DataCollect dataCollect;
        List<DataCollect> dataCollects = await repository.Queryable<DataCollect>().Where(x => x.SetCode == input.SetCode && x.DeleteFlag == 0).ToListAsync();
        if (!dataCollects.Any())
            throw new Exception("当前数据集不存在！");
        else
            dataCollect = dataCollects.First();
        if ("sql" == (input.SetType.IsNull() ? dataCollect.SetType : input.SetType))
        {
            //获取数据源
            List<DataSource> dataSources = await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataCollect.SourceCode).ToListAsync();
            DataSource dataSource = dataSources.FirstOrDefault();
            //获取参数列表
            String dynSql = replaceCustome("sql", dataSource.SourceType, input.SetCode, dataCollect.DynSentence , input.DataSetParamDtoList);
            logger.LogInformation($"[{DateTime.Now}] setcode:[{input.SetCode}] SQL开始执行{dynSql}");
            DateTime dt2 = System.DateTime.Now;
            // 调用服务查询数据
            var tmp = await dataSourceService.testDB(new DataReaderDBTest
            {
                SourceCode = dataSource.SourceCode,
                SourceType = dataSource.SourceType,
                SourceConnect = dataSource.SourceConnect,
                DynSql = dynSql,
            });
            DateTime dt1 = System.DateTime.Now;
            double t = dt1.Subtract(dt).TotalMilliseconds;
            logger.LogInformation($"[{DateTime.Now}]SQL执行时间:{t}");

            return tmp;
        }
        else
        {
            throw new Exception(" http 暂不支持数据源导出 ");
        }
    }

    /// <summary>
    /// 获取count的总数
    /// </summary>
    public async Task<int> testTransform(DataCollectCount input)
    {
        DateTime dt = System.DateTime.Now;
        DataCollect dataCollect;
        List<DataCollect> dataCollects = await repository.Queryable<DataCollect>().Where(x => x.SetCode == input.SetCode && x.DeleteFlag == 0).ToListAsync();
        if (!dataCollects.Any())
            throw new Exception("当前数据集不存在！");
        else
            dataCollect = dataCollects.First();
        if ("sql" == (input.SetType.IsNull() ? dataCollect.SetType : input.SetType))
        {
            //获取数据源
            List<DataSource> dataSources = await repository.Queryable<DataSource>().Where(x => x.SourceCode == dataCollect.SourceCode).ToListAsync();
            DataSource dataSource = dataSources.FirstOrDefault();
            //获取参数列表
            String dynSql = replaceCustome("sql", dataSource.SourceType, input.SetCode, dataCollect.DynSentence, input.DataSetParamDtoList);
            logger.LogInformation($"[{DateTime.Now}] setcode:[{input.SetCode}] SQL开始执行{dynSql}");
            DateTime dt2 = System.DateTime.Now;
            // 调用服务查询数据
            var tmp = await dataSourceService.testDB(new DataCountDBTest
            {
                SourceCode = dataSource.SourceCode,
                SourceType = dataSource.SourceType,
                SourceConnect = dataSource.SourceConnect,
                DynSql = dynSql,
            });
            DateTime dt1 = System.DateTime.Now;
            double t = dt1.Subtract(dt).TotalMilliseconds;
            logger.LogInformation($"[{DateTime.Now}]SQL执行时间:{t}");

            return tmp;
        }
        else
        {
            return -1;
        }
    }
    //-------------------------  以下是外部使用接口

    /// <summary>
    /// 获取caseResult中的所有键
    /// </summary>
    public List<string> getSetParamList(string caseResult, string setType, string setDesc)
    {
        List<String> list = new List<string>();
        try
        {
            var jToken = JToken.Parse(caseResult);
            switch (setType)
            {
                case "sql":
                    foreach (var item in jToken[0])
                    {
                        list.Add(item.ToObject<JProperty>().Name);
                    }
                    break;
                case "http":

                    if (jToken.Type == JTokenType.Array)
                    {
                        foreach (var item in jToken[0])
                        {
                            list.Add(item.ToObject<JProperty>().Name);
                        }
                    }
                    else
                    {
                        String[] arr = setDesc.Split('.');
                        for (int i = 0; i < arr.Length; i++)
                        {
                            jToken = jToken[integerCheck(arr[i]) ? Convert.ToInt32(arr[i]) : arr[i]];
                        }
                        if (jToken.Type == JTokenType.Array && jToken.Count() >= 1)
                        {
                            jToken = jToken[0];
                        }
                        foreach (var item in jToken)
                        {
                            list.Add(item.ToObject<JProperty>().Name);
                        }
                    }
                    //} else {
                    //    dynamic DynamicObject = JsonConvert.DeserializeObject<dynamic>(result.caseResult);

                    //}
                    break;
            }
        }
        catch (Exception) { } // 此处不做动作
        return list;
    }

}

