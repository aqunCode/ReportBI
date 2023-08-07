using System.Text;
using System.Data;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using MagicOnion;
using Bi.Entities.Entity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Bi.Core.Extensions;

namespace Bi.Services.Service;

using SqlSugar;

public class ReportDashBoardServices : IReportDashBoardServices {


    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    private readonly IDataCollectServices dataCollectServices;


    /// <summary>
    /// 构造函数
    /// </summary>
    public ReportDashBoardServices(ISqlSugarClient _sqlSugarClient, IDataCollectServices dataCollectServices)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.dataCollectServices = dataCollectServices;
    }

    public async UnaryResult<DashBoardOutput>  preview(string reportCode)
    {
        DashBoardOutput result = new DashBoardOutput();
        List<ReportDashboard> reportDashboards = await repository.Queryable<ReportDashboard>().Where(x => x.ReportCode == reportCode).ToListAsync();

        if (!reportDashboards.Any())
        {
            return new DashBoardOutput();
        }

        ReportDashboardOutput reportDashboardOutput = reportDashboards.First().MapTo<ReportDashboardOutput>();
        List<ReportDashboardWidgetOutput> list = new List<ReportDashboardWidgetOutput>();

        IEnumerable<ReportDashboardWidget> widgetList =  await repository.Queryable<ReportDashboardWidget>().Where(x => x.ReportCode == reportCode).ToListAsync();  
        foreach (ReportDashboardWidget widget in widgetList)
        {
            ReportDashboardWidgetOutput output = new ReportDashboardWidgetOutput();
            ReportDashboardWidgetValue value = new ReportDashboardWidgetValue();
            value.Setup = Convert.ToString(widget.Setup) != String.Empty? JToken.Parse(widget.Setup) : new JObject();
            value.Data = Convert.ToString(widget.Data) != String.Empty ? JToken.Parse(widget.Data) : new JObject();
            value.Position = !String.IsNullOrEmpty(Convert.ToString(widget.Position)) ? JToken.Parse(widget.Position) : new JObject();
            value.Collapse = !String.IsNullOrEmpty(Convert.ToString(widget.Collapse)) ? JToken.Parse(widget.Collapse) : new JObject();

            output.Value = value;
            output.Type = widget.Type;
            output.Options = widget.Options;
            list.Add(output);
        }

        reportDashboardOutput.Widgets = list;
        result.Dashboard = reportDashboardOutput;
        result.ReportCode = reportCode;
        return result;
    }

    public async UnaryResult<(bool res, string msg)> insert(DashBoardInput input, bool master = true) {

        string reportCode = input.ReportCode;
        bool isAdd = false; 
        List<ReportDashboard> reportDashboards = await repository.Queryable<ReportDashboard>().Where(x => x.ReportCode == reportCode).ToListAsync();
        var dashboard =  input.Dashboard.MapTo<ReportDashboard>();
        dashboard.ReportCode = reportCode;
        if(!reportDashboards.Any())
        {
            dashboard.Create(input.CurrentUser);
            isAdd = true;
        }
        else
        {
            dashboard.Modify(reportDashboards.First().Id,input.CurrentUser);
            isAdd = false;
        }
        //开始计算行列拖拽信息记录在数据库中
        foreach(ReportDashboardWidgetInput item in input.Widgets)
        {
            JToken jtoken = item.Value.Data;
            string tmp;
            AutoTurn at = new AutoTurn();
            if(jtoken != null && jtoken.Select(x => x.ToObject<JProperty>().Name).ToList().Contains("dynamicData"))
            {
                JToken jtoken1 = jtoken["dynamicData"];
                List<string> keys = jtoken1.Select(x => x.ToObject<JProperty>().Name).ToList();
                if(keys.Contains("setCode") && keys.Contains("autoTurn"))
                {
                    tmp = (string)jtoken1["setCode"];
                    at = JsonConvert.DeserializeObject<AutoTurn>(jtoken1["autoTurn"].ToString());
                }
                else { continue; }
            }
            else { continue; }
            if(at == null || (at.Rows == null&& at.Columns == null))
            {
                continue;
            }
            // 将行列拖拽信息存入到数据库中
            List<AutoTurns> turns = new List<AutoTurns>();
            foreach(Column column in at.Rows)
            {
                AutoTurns a = new AutoTurns
                {
                    SetCode = tmp,
                    Turntype = "row",
                    Name = column.Name,
                    CalcType = column.CalcType,
                    Function = column.Function,
                    Value = String.Join(",", column.Values==null?"": column.Values)
                };
                a.Create(input.CurrentUser);
                turns.Add(a);
            }
            foreach(Column column in at.Columns)
            {
                AutoTurns a = new AutoTurns
                {
                    SetCode = tmp,
                    Turntype = "column",
                    Name = column.Name,
                    CalcType = column.CalcType,
                    Function = column.Function,
                    Value = String.Join(",", column.Values == null ? "" : column.Values)
                };
                a.Create(input.CurrentUser);
                turns.Add(a);
            }
            await repository.Deleteable<AutoTurns>().Where(x => x.SetCode == tmp).ExecuteCommandAsync();
            await repository.Insertable<AutoTurns>(turns).ExecuteCommandAsync();
        }
        
        var res = 0;
        try
        {
            repository.Ado.BeginTran();
            if (isAdd)
            {
                res =await repository.Insertable(dashboard).ExecuteCommandAsync();
            }
            else
            {
                res =await repository.Updateable(dashboard).ExecuteCommandAsync();
            }

            res = await repository.Deleteable<ReportDashboardWidget>().Where(x => x.ReportCode == reportCode).ExecuteCommandAsync();

            List<ReportDashboardWidgetInput> widgets = input.Widgets;
            List<ReportDashboardWidget> insertWidgets = new List<ReportDashboardWidget>();
            foreach (var widget in widgets)
            {
                ReportDashboardWidget reportDashboardWidget = new ReportDashboardWidget();
                string type = widget.Type;
                ReportDashboardWidgetValue value = widget.Value;
                reportDashboardWidget.Type = type;
                reportDashboardWidget.ReportCode = reportCode;
                reportDashboardWidget.Setup = value.Setup != null ? Convert.ToString(value.Setup) : string.Empty;
                reportDashboardWidget.Data = value.Data != null ? Convert.ToString(value.Data) : string.Empty;
                reportDashboardWidget.Position = value.Position != null ? Convert.ToString(value.Position) : string.Empty;
                reportDashboardWidget.Collapse = value.Collapse != null ? Convert.ToString(value.Collapse) : string.Empty;
                reportDashboardWidget.Options = widget.Options != null ? Convert.ToString(widget.Options) : string.Empty;
                reportDashboardWidget.DeleteFlag = 0;
                reportDashboardWidget.Enabled = 1;
                reportDashboardWidget.Create(input.CurrentUser);
                insertWidgets.Add(reportDashboardWidget);
            }
            if (insertWidgets.Count() > 0)
            {
                res = await repository.Insertable(insertWidgets).ExecuteCommandAsync();
            }
            repository.Ado.CommitTran();

            if(res > 0)
                return (true, "ok");
            return (false, $"fail");
        }
        catch (Exception e)
        {
            string msg =  e.Message.ToString();
            repository.Ado.RollbackTran();
            return (false, e.Message);
        }
    }

    public async UnaryResult<string> getChartData(ChartInput input)
    {
        DataCollectInput dataCollectInput = getDataSet(input, 1, int.MaxValue);
        var result =  await dataCollectServices.testTransform(dataCollectInput);
      //  List<JObject> data = JArray.Parse(jsonStr).ToObject<List<JObject>>();
        return result.Item1;
    }

    DataCollectInput getDataSet(ChartInput input, int requestCount, int pageSize)
    {
        DataCollectInput dto = new DataCollectInput();
        dto.SetCode = input.SetCode;
        if (requestCount <= 0) requestCount = 1;
        if (pageSize <= 0) pageSize =1;
        dto.LimitStart =(requestCount - 1)* pageSize +1;
        dto.LimitEnd = (requestCount) *pageSize;
        getContextData(input.ContextData, dto);
        return dto;
    }

    private void getContextData(Dictionary<string, string> setParam, DataCollectInput dto)
    {
        List<DataCollectItem> list = new List<DataCollectItem>();
        // 查询条件
        foreach(KeyValuePair<string, string> items in setParam)
        {
            DataCollectItem item = new DataCollectItem();
            item.ParamName = items.Key;
            item.SampleItem = items.Value;
            list.Add(item);
        }
        dto.DataSetParamDtoList = list;
    }

    /// <summary>
    /// 数据二次处理
    /// </summary>
    /// <param name="result"></param>
    /// <param name="autoTurn"></param>
    /// <param name="setCode"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async UnaryResult<JToken> turnData(string setCode, string result, AutoTurn autoTurn)
    {
        #region 行列拖拽信息的check
        if(autoTurn == null)
        {
            //从数据库查询有拖拽信息
            var turns = await repository.Queryable<AutoTurns>().Where(x => x.SetCode == setCode && x.Enabled == 1).ToListAsync();
            if(turns == null || turns.Count() == 0)
            {
                throw new Exception("请选择所需的行列数据！");
            }
            autoTurn = new AutoTurn();
            var rows = turns.Where(x => x.Turntype == "row");
            var columns = turns.Where(x => x.Turntype == "column");
            autoTurn.Rows = new List<Column>();
            foreach(var row in rows)
            {
                string[] arr = row.Value.Split(',');
                autoTurn.Rows.Add( new Column
                {
                    Name = row.Name,
                    Function = row.Function,
                    Values = row.Value.Split(',')
                });
            }
            autoTurn.Columns = new List<Column>();
            foreach(var column in columns)
            {
                autoTurn.Columns.Add(new Column
                {
                    Name = column.Name,
                    Function = column.Function,
                    Values = column.Value.Split(',')
                });
            }
        }
        #endregion
        JToken jToken = JToken.Parse(result);
        #region json 检查
        if(jToken.Type != JTokenType.Array){ throw new Exception("原始数据格式错误！"); }
        if(jToken.Count() == 0) { throw new Exception("原始数据为空！"); }
        // 所有的key值
        var checkOne = jToken[0];
        var rowList = autoTurn.Rows.Where(x=> x.CalcType.IsNullOrEmpty() || x.CalcType == "normal")
            .Select(x => x.Name).ToList();
        var columnList = autoTurn.Columns.Where(x => x.CalcType.IsNullOrEmpty() || x.CalcType == "normal")
            .Select(x => x.Name).ToList();
        List<string> list = new List<string>();
        list.AddRange(rowList);
        list.AddRange(columnList);
        Boolean flag = checkOne.Select(x => x.ToObject<JProperty>().Name).ToList()
            .ContainsAll(list.ToArray()) ;
        if(!flag) { throw new Exception("拖拽行列不在原始数据中");}
        #endregion
        // 可以开始计算了

        // 筛选所有的自定义语法
        var customerCalc = autoTurn.Rows.Where(x => x.Function.IsNotNullOrEmpty() && x.CalcType == "calculator").ToList();
        customerCalc.AddRange(autoTurn.Columns.Where(x => x.Function.IsNotNullOrEmpty() && x.CalcType == "calculator").ToList());
        // 筛选所有需要计算的行列信息 count min max distinct
        var calc =  autoTurn.Rows.Where(x => x.Function.IsNotNullOrEmpty() && x.CalcType != "calculator").ToList();
        calc.AddRange( autoTurn.Columns.Where(x => x.Function.IsNotNullOrEmpty() && x.CalcType != "calculator").ToList() );

        #region 从原始字段result 中筛选出所需的所有字段
        Dictionary<string, List<String>> dic = new Dictionary<string, List<String>>();
        foreach(var item in list)
        {
            var res = from p in jToken
                      select (string)p[item];
            dic.Add(item, res.ToList());
        }
        #endregion
        if(calc.Count()  == 0)
        {//如果不包含计算结果，则返回table表
            //转化数据格式，返回值 
            JArray rss = new JArray();
            for(int i = 0; i < jToken.Count(); i++)
            {
                JObject obj = new JObject();
                for(int j = 0; j < list.Count(); j++)
                {
                    obj.Add(
                        new JProperty(list[j],dic[list[j]][i]));
                }
                rss.Add(obj);
            }
            JObject rss2 = new JObject(
                    new JProperty("chartProperties",
                        new JObject(
                            new JProperty("rows",
                                new JArray(
                                    from c in rowList
                                    select new JValue(c)
                                    )
                            ),
                            new JProperty("columns",
                                new JArray(
                                    from c in columnList
                                    select new JValue(c)
                                    )
                            )
                        )
                    ),
                    new JProperty("data", rss)
                );
            return rss2;
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string,Object> calculator = new Dictionary<string, Object>();
            var cacList = calc.Select(x => x.Name).ToList();
            var cusCalcList = customerCalc.Select(x => x.Name).ToList();
            list.RemoveIf(x=> cacList.Contains(x));
            list.RemoveIf(x=> cusCalcList.Contains(x));

            // 优先开始自定义语法计算
            for(int j = 0; j < customerCalc.Count(); j++)
            {
                customerFunction(jToken,dic, customerCalc[j]);
            }

            for(int i = 0; i < jToken.Count(); i++)
            {
                sb.Clear();
                for(int j = 0; j < list.Count(); j++)
                {
                    sb.Append(dic[list[j]][i]);
                    sb.Append(',');
                }
                foreach(var item in calc)
                {
                    var key = sb.ToString()+item.Name;

                    switch(item.Function)
                    {
                        // 这里添加 计算方式，可以实现自定义的计算方式
                        case "Count":
                            calculator[key] = (int)calculator.GetOrAdd(key, 0) + 1;
                            break;
                        case "CountDistinct":
                            var hashSet = (HashSet<string>)calculator.GetOrAdd(key, new HashSet<string>());
                            hashSet.Add(dic[item.Name][i]);
                            calculator[key] = hashSet;
                            break;
                    }
                }
            }
            // 将 calculator 转换为 JArray
            JArray rss = new JArray();
            int cacIndex = 0;
            JObject cacj = new JObject();
            foreach(var item in calculator)
            {
                if(cacIndex == 0)
                {
                    String[] arr = item.Key.Split(',');
                    for(int i = 0 ; i < arr.Length-1; i++)
                    {
                        cacj.Add(
                        new JProperty(list[i],arr[i] ));
                    }
                    
                }
                switch(calc[cacIndex].Function)
                {
                    // 这里根据不同的计算方式来记录结果
                    case "Count":
                        cacj.Add(
                            new JProperty(cacList[cacIndex], item.Value));
                        break;
                    case "CountDistinct":
                        var res = (HashSet<string>)item.Value;
                        cacj.Add(
                            new JProperty(cacList[cacIndex], res.Count()));
                        break;
                }
                cacIndex++;
                
                if(cacIndex == cacList.Count())
                {
                    cacIndex = 0;
                    rss.Add(cacj);
                    cacj = new JObject();
                }
            }
            #region 使用Jtoken 返回固定格式的结果集
            JObject rss2 = new JObject(
                    new JProperty("chartProperties",
                        new JObject(
                            new JProperty("rows",
                                new JArray(
                                    from c in rowList
                                    select new JValue(c)
                                    )
                            ),
                            new JProperty("columns",
                                new JArray(
                                    from c in columnList
                                    select new JValue(c)
                                    )
                            )
                        )
                    ),
                    new JProperty("data", rss)
                );
            #endregion

            return rss2;
        }
    }
    // 自定义语法解析计算
    private void customerFunction(JToken jToken, Dictionary<string, List<String>> dic,Column item)
    {
        string code = item.Function;
        // 收集 item.function [] 中的字段
        List<string> list = getList(code);
        foreach (string s in list)
        {
            if(dic[s]== null)
            {
                var res = from p in jToken
                          select (string)p[s];
                dic.Add(s, res.ToList());
            }
        }
        // 开始解析 算是解释执行吧
        StringBuilder sb = new StringBuilder();
        //string forecast = "";
        for(int i = 0; i < code.Length; i++)
        {
            switch(code[i])
            {
                case '{':   // FIXED
                    //forecast = "fixed";
                    break;
                case ' ':

                    break;
                default:
                    sb.Append(code[i]);
                    break;
            }
        }
    }
    // 获取所有的字段列表
    private List<string> getList(string function)
    {
        List<string> list = new List<string>();
        for(int i = 0; i < function.Length; i++)
        {
            if(function[i] == '[')
            {
                list.Add(function.Substring(i+1,function.IndexOf(']', i + 1)-i-1));
            }
        }
        return list;
    }

    private List<int> analysisBlock(string function)
    {
        List<int> list = new List<int>();
        if(function.LastIndexOf('(') == -1)
        {
            list.Add(0);
            list.Add(function.Length - 1);
            list.Add(1);
            return new List<int>();
        }
        else
        {
            // 首先判断括号，优先执行括号中的内容
            return null;
        }
    }
}
