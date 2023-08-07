using System.Text;
using OfficeOpenXml;
using System.Data;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using MagicOnion;
using Bi.Entities.Entity;
using Newtonsoft.Json.Linq;

namespace Bi.Services.Service;

using System.Collections.Concurrent;
using Mapster;
using Microsoft.ClearScript.V8;
using System.Text.RegularExpressions;
using MiniExcelLibs;
using Microsoft.Extensions.Logging;
using ICSharpCode.SharpZipLib.Zip;
using SqlSugar;
using Bi.Core.Extensions;
using Bi.Core.Const;

public class ReportExcelService : IReportExcelService
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    private readonly IDataCollectServices dataCollectServices;

    private readonly IDynamicCodeService dynamicCodeService;

    private readonly ILogger<ReportExcelService> logger;
    /// <summary>
    /// 调用数据集的参数集
    /// </summary>
    private ConcurrentDictionary<string, (DataTable, long, long)> keyValuePairs = new ();


    /// <summary>
    /// 构造函数
    /// </summary>
    public ReportExcelService(ISqlSugarClient _sqlSugarClient,
                                IDataCollectServices dataCollectServices, 
                                IDynamicCodeService dynamicCodeService,
                                ILogger<ReportExcelService> logger)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.dataCollectServices = dataCollectServices;
        this.dynamicCodeService = dynamicCodeService;
        this.logger = logger;
    }
    /// <summary>
    /// 单个添加
    /// </summary>
    public async UnaryResult<(bool res, string msg)> AddAsync(IEnumerable<ReportExcelInput> input)
    {
        var currentUser = input.First().CurrentUser;
        var entities = input.MapTo<ReportExcel>();
        // 先删后加
        foreach(var entity in entities)
        {
            int res = await repository.Deleteable<ReportExcel>().Where(x => x.ReportCode == entity.ReportCode).ExecuteCommandAsync();
        }
        entities.ForEach(entity =>
        {
            entity.Create(currentUser);
        });
        if ((await repository.Insertable(entities).ExecuteCommandAsync()) > 0)
            return (true, "ok");
        return (false, $"fail");
    }
    /// <summary>
    /// 单个修改
    /// </summary>
    public async UnaryResult<(bool res, string msg)> ModifyAsync(ReportExcelInput input)
    {
        var entitys = await repository.Queryable<ReportExcel>().Where(x => x.ReportCode == input.ReportCode).ToListAsync();
        if (entitys == null)
        {
            return (false, $"fail");
        }
        var entity = input.MapTo<ReportExcel>();
        entity.Modify(entitys.First().Id, input.CurrentUser);

        if ((await repository.Updateable(entity).ExecuteCommandAsync()) > 0)
            return (true, "ok");
        return (false, $"fail");
    }
    /// <summary>
    /// 获取单个报表明细
    /// </summary>
    public async UnaryResult<ReportExcelOutput> detailByReportCode(string reportCode, bool master = true)
    {
        List<ReportExcel> reportExcels = await repository.Queryable<ReportExcel>().Where(x => x.ReportCode == reportCode).ToListAsync();
        if (reportExcels.Any())
        {
            return reportExcels.First().MapTo<ReportExcelOutput>();
        }
        return null;
    }
    
    /// <summary>
    /// excel 导出
    /// </summary>
    public async Task<(string, string)> exportExcel(ReportExcelInput input)
    {
        DateTime dt = System.DateTime.Now;

        ReportExcelOutput reportExcelOutput = await getResEntity(input);
        JObject sheetItem = JToken.Parse(reportExcelOutput.JsonStr)[0].ToObject<JObject>();
        sheetItem.Remove("data");

        // 获取所有的单元格信息
        var cellList = getSheetItems(sheetItem);
        foreach (var cell in cellList)
            cell.Original = null;

        // 区分动态单元格和静态单元格
        var dynEnums = cellList.Where(x => x.SingleCellType == CellType.DYNAMIC || x.SingleCellType == CellType.DYNAMIC_MERGE);
        var staticEnums = cellList.Where(x => x.SingleCellType != CellType.DYNAMIC && x.SingleCellType != CellType.DYNAMIC_MERGE);

        var (path,fileName) = ("","");
        switch (input.ExportType)
        {

            case "1":                                   // 数据源直接导出
                (path, fileName) = await exportDataReader(reportExcelOutput);
                break;
            case "2":                                   // 快速导出
                // 优先加载数据源
                loadingData(sheetItem, cellList, input.SetParam, input.RequestCount, input.PageSize, true,"2");
                (path, fileName) = exportDataTableByTemplate(dynEnums,staticEnums);
                break;
            case "3":                                   // 根据设定模板导出
                // 标记所有的最父格
                checkFinalFather(cellList);
                // 优先加载数据源
                loadingData(sheetItem, cellList, input.SetParam, 1, int.MaxValue, false, "3");

                // 用于结果集
                List<CellItem> resultCelldata = new List<CellItem>();
                // 记录结果集的行列信息
                Dictionary<string, int> indexRecord = initRecord(cellList);
                // 获取块信息
                JArray relationList = (JArray)sheetItem["relationList"] ?? new JArray();
                // 逐行逐列枚举单元格
                foreach (CellItem cellItem in cellList)
                {
                    if (indexRecord.ContainsKey(cellItem.Row + "-" + cellItem.Column)) { continue; }
                    // 开始加载单元格
                    analysisCellData(relationList, cellList, cellItem, resultCelldata, indexRecord);
                }// 合并单元格
                mergeCellAll(resultCelldata, indexRecord);
                // 补充空白行
                replenish(sheetItem, cellList, resultCelldata);
                // 判断字段属性
                checkValueType(resultCelldata);
                // 开始计算函数单元格函数单元格
                calculatorFunction(cellList, resultCelldata);
                // 单元格样式克隆
                styleCloneAll(sheetItem, resultCelldata, indexRecord);
                // 自定义语法调用
                var res = await executeMonacoEditor(resultCelldata, sheetItem.SelectToken("monacoEditor"));
                List<CellItem> sjArray =
                    (from c in resultCelldata
                     orderby c.Row ascending, c.Column ascending
                     select c).ToList();
                (path, fileName) = await exportDefaultExcel(resultCelldata);
                break;
            case "4":
                loadingData(sheetItem, cellList, input.SetParam, input.RequestCount, input.PageSize, true, "2");
                (path, fileName) = exportDataTableBySheet(dynEnums, staticEnums);
                break;
            default:                                   // 数据源直接导出
                (path,fileName) = await exportDataReader(reportExcelOutput);
                break;
        }
        // 参数集清空，结果集清空,清除缓存信息
        keyValuePairs = new ConcurrentDictionary<string, (DataTable, long, long)>();
        // 临时文件清除
        if (Directory.Exists(path))
        {
            DirectoryInfo dic = new DirectoryInfo(path);
            var allFile = dic.GetFiles();
            foreach(var file in allFile)
            {
                if(file.LastWriteTime < DateTime.Now.AddHours(-24))
                {
                    file.Delete();
                }
            }
        }

        double between = System.DateTime.Now.Subtract(dt).TotalMilliseconds;
        return (path, fileName);
    }

    private async Task<bool> executeMonacoEditor(List<CellItem> resultCelldata, JToken jToken)
    {
        if (jToken == null)
            return true;
        var res = await dynamicCodeService.syntaxRules(new DynamicCodeInput
        {
            List = resultCelldata,
            DynamicCode = (string)jToken,
            CheckFlag = false
        });
        if (!res.Item2)
        {
            throw new Exception("自定义语法执行错误:"+res.Item1);
        }
        return res.Item2;
    }

    /// <summary>
    /// miniExcel 导出之 IDataReader 导出
    /// </summary>
    private async Task<(string,string)> exportDataReader(ReportExcelOutput reportExcelOutput)
    {
        var templatePath = AppDomain.CurrentDomain.BaseDirectory + $"excel/";
        var fileName = $"{Guid.NewGuid()}.csv";
        if (!Directory.Exists(templatePath))
        {
            Directory.CreateDirectory(templatePath);
        }

        // 获取前台查询条件
        List<DataCollectItem> list = getContextData(reportExcelOutput.SetParam, reportExcelOutput.SetCodes);
        
        if(reportExcelOutput.SetCodes.IndexOf("|") != -1)
        {
            throw new Exception(" 多个数据集无法导出数据源 ");
        }

        // 获取数据源 DataReader 
        //(IDataReader, string) result = (null, "a809feb1-1d3e-43b7-a451-454988dd4f30.csv");
        var result = await dataCollectServices.testTransform(new DataCollectReader
        {
            SetCode = reportExcelOutput.SetCodes,
            DataSetParamDtoList = list
        });
        try
        {
            if (result.Item2.IsNullOrEmpty())
            {
                var config = new MiniExcelLibs.Csv.CsvConfiguration();
                await MiniExcel.SaveAsAsync(templatePath + fileName, result.Item1, configuration: config);
                //根据情况判断是否压缩文件
                (templatePath, fileName) = zipFile(templatePath, fileName);
                return (templatePath, fileName);
            }
            else
            {
                (templatePath, fileName) = zipFile(templatePath, result.Item2);
                return (templatePath, fileName);
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            result.Item1?.Close();
            result.Item1?.Dispose();
        }
    }

    /// <summary>
    /// 单文件压缩
    /// </summary>
    /// <param name="sourceFile">源文件路径</param>
    /// <param name="fileName">源文件路径</param>
    /// <param name="targetZipFile">zip压缩文件</param>
    /// <param name="compressionLevel">压缩级别</param>
    public int ZipFile(string sourceFile,string fileName, string targetZipFile,int compressionLevel = 6)
    {
        if (!File.Exists(sourceFile))
        {
            return 0;
        }

        FileStream streamToZip = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
        FileStream zipFile = File.Create(targetZipFile);
        ZipOutputStream zipStream = new ZipOutputStream(zipFile);

        ZipEntry zipEntry = new ZipEntry(fileName);
        zipStream.PutNextEntry(zipEntry);

        //存储、最快、较快、标准、较好、最好  0-9
        zipStream.SetLevel(compressionLevel);

        try
        {
            // 方法主体部分
            streamToZip.CopyTo(zipStream);
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
        {
            // 异常处理代码
            return 0;
        }
        finally
        {
            zipStream?.Finish();
            zipStream?.Close();
            zipFile?.Dispose();
            streamToZip?.Close();
        }
        return 1;
    }
    /// <summary>
    /// 压缩文件
    /// </summary>
    private (string templatePath, string fileName) zipFile(string templatePath, string fileName)
    {
        // 首先判断文件大小
        FileInfo info = new FileInfo(templatePath+ fileName);
        if(info.Exists && info.Length > 10 * 1024 * 1024)
        {
            var newFileName = $"{Guid.NewGuid()}.zip";
            var status = ZipFile(templatePath + fileName, fileName , templatePath+ newFileName);
            if(status == 1)
                return (templatePath, newFileName);
            else
                return (templatePath, fileName);
        }
        return (templatePath, fileName);
    }

    /// <summary>
    /// miniExcel 导出之 DataTable 模板导出
    /// </summary>
    private (string,string) exportDataTableByTemplate(IEnumerable<CellItem> dynEnums, IEnumerable<CellItem> staticEnums)
    {
        DataTable dt;
        var TemplatePath = AppDomain.CurrentDomain.BaseDirectory + $"excel/";
        var fileName = $"{Guid.NewGuid()}.xlsx";

        // 首先使用Epplus 创建模板
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (ExcelPackage excelPackage = new ExcelPackage())
        {
            ExcelWorksheet sheet = excelPackage.Workbook.Worksheets.Add("sheet1");
            // 首先填写静态值
            foreach (CellItem item in staticEnums)
            {
                sheet.SetValue(item.Row + 1, item.Column + 1, item.Value);
            }
            excelPackage.SaveAs(string.Concat(TemplatePath, fileName));
        }

        IEnumerable<string> codes = dynEnums.Select(x => x.SetCode).Distinct();

        var memoryStream = new MemoryStream();

        foreach (var code in codes)
        {
            using (ExcelPackage excelPackage = new ExcelPackage(string.Concat(TemplatePath, fileName)))
            {
                ExcelWorksheet sheet = excelPackage.Workbook.Worksheets["sheet1"];
                dt = getData(new DataCollectInput { SetCode = code }).Item1;

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    foreach (var item in dynEnums)
                    {
                        if (item.SetKey.ToLower() == dt.Columns[i].ColumnName.ToLower())
                        {
                            sheet.SetValue(item.Row + 1, item.Column + 1, $"{{{{managers.{dt.Columns[i].ColumnName}}}}}");
                        }
                    }
                }
                excelPackage.Save();
            }
            var Value = new Dictionary<string, object>()
            {
                ["managers"] = dt
            };
            string newFileName = $"{Guid.NewGuid()}.xlsx";

            MiniExcel.SaveAsByTemplate(string.Concat(TemplatePath, newFileName), string.Concat(TemplatePath, fileName), Value);
            fileName = newFileName;
        }
        return (TemplatePath, fileName);
    }

    /// <summary>
    /// Epplus 导出 分多sheet,
    /// </summary>
    /// <param name="dynEnums"></param>
    /// <param name="staticEnums"></param>
    /// <param name="size">当超过1000000时,分Sheet导出</param>
    /// <returns></returns>
    private (string,string) exportDataTableBySheet(IEnumerable<CellItem> dynEnums, IEnumerable<CellItem> staticEnums,int size=1000000)
    {
        // 首先使用Epplus 创建模板
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        DataTable dt;
        var TemplatePath = AppDomain.CurrentDomain.BaseDirectory + $"excel/";
        var fileName = $"{Guid.NewGuid()}.xlsx";

        string codetest = dynEnums.Select(x => x.SetCode).FirstOrDefault();
        dt = getData(new DataCollectInput { SetCode = codetest }).Item1;

        var pages = dt.Rows.Count / size + (dt.Rows.Count % size > 0 ? 1 : 0);

        if (dt.Rows.Count > 0)
        {
            using var package = new ExcelPackage(new FileInfo(string.Concat(TemplatePath, fileName)));
            for (var page = 0; page < pages; page++)
            {
                var sheet = package.Workbook.Worksheets.Add($"Sheet{page + 1}");
                sheet.Cells["A1"].LoadFromDataTable(dt.AsEnumerable().Skip(page * size).Take(size).CopyToDataTable(), true);
                //单元格自动适应大小
                sheet.Cells.AutoFitColumns();
                //单独设置单元格
                //action?.Invoke(sheet);
            }
            package.Save();
        }

        return (TemplatePath, fileName);

    }

    /// <summary>
    /// Epplus    导出之 List<CellItem/> 导出
    /// </summary>
    private async Task<(string,string)> exportDefaultExcel(List<CellItem> resultCelldata)
    {
        var TemplatePath = AppDomain.CurrentDomain.BaseDirectory + $"excel/";
        var fileName = $"{Guid.NewGuid()}.xlsx";
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (ExcelPackage excelPackage = new ExcelPackage())
        {
            ExcelWorksheet sheet = excelPackage.Workbook.Worksheets.Add("sheet1");
            foreach (CellItem item in resultCelldata)
            {
                sheet.SetValue(item.Row+1, item.Column+1, item.Value);
            }
            await excelPackage.SaveAsAsync( string.Concat(TemplatePath, fileName));
        }
        return (TemplatePath, fileName);
    }

    /// <summary>
    /// excel 初次预览
    /// </summary>
    public async Task<ReportExcelOutput> firstPreview(ReportExcelInput input)
    {
        return await getResEntity(input);
    }
    /// <summary>
    /// 记录sql总数
    /// </summary>
    public async Task<ReportExcelOutput> countPreview(ReportExcelInput input)
    {
        ReportExcelOutput reportExcelOutput = await getResEntity(input);
        // 获取前台查询条件
        List<DataCollectItem> list = getContextData(reportExcelOutput.SetParam, reportExcelOutput.SetCodes);

        if (reportExcelOutput.SetCodes.IndexOf("|") != -1)
        {
            reportExcelOutput.SetCodes = reportExcelOutput.SetCodes.Split('|')[0];
        }

        // 获取数据源  
        var total = await dataCollectServices.testTransform(new DataCollectCount
        {
            SetCode = reportExcelOutput.SetCodes,
            DataSetParamDtoList = list
        });
        reportExcelOutput.Total = total;
        return reportExcelOutput;
    }
    /// <summary>
    /// excel 预览
    /// </summary>
    public async UnaryResult<ReportExcelOutput> preview(ReportExcelInput input)
    {
        DateTime dt = System.DateTime.Now;

        ReportExcelOutput reportExcelOutput = await getResEntity(input);
        reportExcelOutput.JsonStr = await analysisReportData(reportExcelOutput);
        
        // 参数集清空，结果集清空,清除缓存信息
        keyValuePairs = new ConcurrentDictionary<string, (DataTable, long, long)>();

        reportExcelOutput.TimeSpan = System.DateTime.Now.Subtract(dt).TotalMilliseconds;

        return reportExcelOutput;

    }
    /// <summary>
    /// 创建返回值对象
    /// </summary>
    private async Task<ReportExcelOutput> getResEntity(ReportExcelInput input)
    {
        ReportExcelOutput reportExcelOutput;

        // 根据报表编码获取excel 报表
        var reportExcels = await repository.Queryable<ReportExcel>().Where(x => x.ReportCode == input.ReportCode).ToListAsync();
        if (!reportExcels.Any())
            return new ReportExcelOutput();
        reportExcelOutput = reportExcels.First().MapTo<ReportExcelOutput>();

        // 根据报表设计的编码获取对应的名称
        List<DataCollect> tmp;
        foreach (string x in reportExcelOutput.SetCodes.Split('|'))
        {
            tmp = await repository.Queryable<DataCollect>().Where(c => c.SetCode == x).ToListAsync();
            reportExcelOutput.SetNames += tmp.FirstOrDefault().SetName + "|";
        }

        reportExcelOutput.SetNames = reportExcelOutput.SetNames.Substring(0, reportExcelOutput.SetNames.Length - 1);
        reportExcelOutput.SheetIndex = input.SheetIndex;
        reportExcelOutput.RequestCount = input.RequestCount;
        reportExcelOutput.PageSize = input.PageSize;
        reportExcelOutput.Export = input.Export;

        // 设定返回值的参数
        if (!string.IsNullOrEmpty(input.SetParam) && input.SetParam != "{}")
        {
            reportExcelOutput.SetParam = input.SetParam;
        }
        return reportExcelOutput;
    }
    /// <summary>
    /// excel 预览 逐个sheet加载
    /// </summary>
    private async Task<string> analysisReportData(ReportExcelOutput reportExcelOutput)
    {
        JToken excelJson = JToken.Parse(reportExcelOutput.JsonStr);
        // 不同sheet 
        if (excelJson.Type == JTokenType.Array)
        {
            foreach (JObject sheetItem in excelJson)
            {
                // status 为1 的为当前显示的sheet
                if (!reportExcelOutput.Export
                    && reportExcelOutput.SheetIndex.IsNullOrEmpty()
                    && sheetItem["status"].ToString() == "0")
                {
                    sheetItem["celldata"] = new JArray();
                    continue;
                }
                // 当前sheet 不是要查询的sheet
                if (!reportExcelOutput.Export
                    && reportExcelOutput.SheetIndex.IsNotNullOrEmpty()
                    && sheetItem["index"].ToString() != reportExcelOutput.SheetIndex)
                {
                    sheetItem["celldata"] = new JArray();
                    sheetItem["status"] = 0;
                    continue;
                }
                // 删除子节点 data
                sheetItem.Remove("data");

                await analysisSheetCellData(sheetItem,
                    reportExcelOutput.SetParam,
                    reportExcelOutput.RequestCount,
                    reportExcelOutput.PageSize,
                    reportExcelOutput.Export);
            }
        }
        return excelJson.ToJson();
    }
    /// <summary>
    /// excel 预览  对sheet的每个cell 进行数据加载
    /// </summary>
    private async Task<bool> analysisSheetCellData(JObject sheetItem, string setParam, int requestCount, int pageSize, bool export)
    {
        // 判断是否包含 celldata ，感觉很多余
        if (sheetItem.Type == JTokenType.Object
            && sheetItem.ToObject<JObject>().ContainsKey("celldata"))
        { 
            // 获取所有的单元格信息
            var cellList = getSheetItems(sheetItem);
            var cellListBack = cellList.DeepClone();
            foreach (var cell in cellList)
                cell.Original = null;
            // 记录结果集的行列信息
            Dictionary<string, int> indexRecord = initRecord(cellList);
            // 标记所有的最父格
            checkFinalFather(cellList);
            // 用于结果集
            List<CellItem> resultCelldata = new List<CellItem>();
            // 获取块信息
            JArray relationList = (JArray)sheetItem["relationList"]??new JArray();
            // 优先加载数据源
            loadingData(sheetItem,cellList, setParam, requestCount, pageSize, export,null);
            // 逐行逐列枚举单元格
            foreach (CellItem cellItem in cellList)
            {
                if(indexRecord.ContainsKey(cellItem.Row+"-"+ cellItem.Column)) { continue; }
                // 开始加载单元格
                analysisCellData(relationList,cellList, cellItem, resultCelldata, indexRecord);
            }
            // 合并单元格
            mergeCellAll(resultCelldata, indexRecord);
            // 补充空白行
            replenish(sheetItem, cellList, resultCelldata);
            // 判断字段属性
            checkValueType(resultCelldata);
            // 开始计算函数单元格函数单元格
            calculatorFunction(cellList, resultCelldata);
            // 单元格克隆
            styleCloneAll(sheetItem, resultCelldata, indexRecord);
            // 执行自定义语法
            var res = await executeMonacoEditor(resultCelldata, sheetItem.SelectToken("monacoEditor"));
            IEnumerable<CellItem> sjArray =
                (from c in resultCelldata
                    orderby c.Row ascending, c.Column ascending
                    select c).Where(x=>x.Row < 200);
            sheetItem["celldata"] = new JArray(
                                from c in sjArray
                                select new JObject(c.toJson(getOriginal(cellListBack, c.CoordinateRow,c.CoordinateColumn)))
                                );
            sheetItem["status"] = 1;

        }
        return true;
    }
    /// <summary>
    /// 初始化indexRecord
    /// </summary>
    private Dictionary<string, int> initRecord(List<CellItem> cellList)
    {
        // 获取最大行
        int maxRow = cellList.Select(x => x.Row).Max();
        // 获取最大列
        int maxColumn = cellList.Select(x => x.Column).Max();

        Dictionary<string, int> indexRecord = new Dictionary<string, int>();

        for(int i = 0;i<=maxRow ; i++)
        {
            indexRecord["RowInit" + i] = i;
            indexRecord["RowRecord" + i] = 1;
        }

        for (int i = 0; i <= maxColumn; i++)
        {
            indexRecord["ColumnInit" + i] = i;
            indexRecord["ColumnRecord" + i] = 1;
        }

        return indexRecord;
    }

    private JObject getOriginal(List<CellItem> cellList,int CoordinateRow, int CoordinateColumn)
    {
        return cellList.Where(x=> x.CoordinateRow == CoordinateRow && x.CoordinateColumn == CoordinateColumn).First().Original;
    }
    /// <summary>
    /// 空白格函数计算
    /// </summary>
    private void calculatorFunction(List<CellItem> cellList, List<CellItem> resultCelldata)
    {
        bool flag;                              // 判断是行一致还是列一致
        string[] arr = new string[] {"=SUM(","=COUNT(","=AVERAGE(","=MAX(","=MIN(" };
        IEnumerable<CellItem> tmpList;          // 获取需要计算的同行或者同列的单元格
        IEnumerable<int> indexList;             // 获取需要计算所有行，或者列
        IEnumerable<CellItem> calcSetList;      // 获取的需要计算的设定单元格
        IEnumerable<CellItem> calcList;         // 获取的需要计算的单元格
        List<string> tmp = new ();              // 获取需要计算的数据
        List<CellItem> finaAdd = new();

        // 需要计算的字符串
        foreach (var item in cellList)
        {
            foreach(string str in arr)
            {
                if (item.SetValue != null && item.SetValue.ToUpper().IndexOf(str)!=-1)
                {
                    // 说明是函数，需要单独计算
                    string function = item.SetValue.Trim().ToUpper().Replace(str, "");
                    function = function.Substring(0,function.Length-1);

                    // 取出结果集的数据集
                    var itemRes = resultCelldata.Where(x => x.CoordinateRow == item.CoordinateRow && x.CoordinateColumn == item.CoordinateColumn).First();

                    //判断是否是单个
                    if (functionCheck(function))
                    {
                        var (Row ,Column) = translateCoordinate(function);
                        var cell = cellList.Where(x=> x.Row == Row && x.Column == Column).First();

                        if(    cell.SingleCellType == CellType.STATIC 
                            || cell.SingleCellType == CellType.STATIC_MERGE
                            || (item.CoordinateRow != Row && item.CoordinateColumn != Column)
                            || (item.Row == Row && cell.Expend == "cross" && (cell.LeftParentRow == -1 || cell.TopParentRow == -1 ) )
                            || (item.Column == Column && cell.Expend == "portrait" && (cell.LeftParentRow == -1 || cell.TopParentRow == -1)))
                        {
                            string Value = calculatorFunctionValue(resultCelldata.Where(x => x.CoordinateRow == Row && x.CoordinateColumn == Column && x.Value.IsNotNullOrEmpty()).Select(x => x.Value), str);
                            itemRes.Value = Value;   
                        }else if(cell.SingleCellType == CellType.DYNAMIC || cell.SingleCellType == CellType.DYNAMIC_MERGE)
                        {
                            // 此情况需跟随复制计算逻辑
                            calcList = resultCelldata.Where(x => x.CoordinateRow == Row && x.CoordinateColumn == Column);

                            flag = item.Row == cell.Row;
                            indexList = calcList.Select(x => flag?x.Row:x.Column).Distinct();
                            
                            // 此处开启循环追加添加单元格
                            foreach (var calcIndex in indexList)
                            {
                                tmp.Clear();
                                var itemTmp = itemRes.DeepClone();
                                if (flag)
                                    tmpList = calcList.Where(x => x.Row == calcIndex);
                                else
                                    tmpList = calcList.Where(x=> x.Column == calcIndex);

                                if (cell.Expend == "no")
                                {
                                    foreach(var RowItem in tmpList)
                                    {
                                        tmp.AddRange( RowItem.Value.Split(','));
                                    }
                                }
                                else
                                {
                                    tmp.AddRange(tmpList.Select(x => x.Value));
                                }

                                // 追加函数计算的单元格
                                if(flag && itemTmp.Row == calcIndex
                                    || !flag && itemTmp.Column == calcIndex)
                                {
                                    itemRes.Value = calculatorFunctionValue(tmp.Where(x=>x.IsNotNullOrEmpty()), str);
                                }
                                else
                                {
                                    if (flag)
                                        itemTmp.Row = calcIndex;
                                    else
                                        itemTmp.Column = calcIndex;
                                    itemTmp.Value = calculatorFunctionValue(tmp.Where(x => x.IsNotNullOrEmpty()), str);
                                    finaAdd.Add(itemTmp);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 此处说明是区间函数，此处设定，除非同在同一行，且拓展方向一致才会全部计算
                        if (function.IndexOf(':') != -1 && functionCheck(function.Replace(":","")))
                        {
                            // 坐标转换
                            string[] arrTmp = function.Split(':');
                            var (Row1,Column1) = translateCoordinate(arrTmp[0]);
                            var (Row2, Column2) = translateCoordinate(arrTmp[1]);

                            // 获取所有的单元格
                            calcSetList = cellList.Where(x => x.Row >= Row1 && x.Row <= Row2
                                                    && x.Column >= Column1 && x.Column <= Column2);
                            calcList = resultCelldata.Where(x => x.CoordinateRow >= Row1 && x.CoordinateRow <= Row2
                                                    && x.CoordinateColumn >= Column1 && x.CoordinateColumn <= Column2);

                            if(calcSetList.Where(x=>x.SingleCellType == CellType.STATIC || x.SingleCellType == CellType.STATIC_MERGE).Any()
                               || Row1 != Row2 && Column1 != Column2
                               || Row1 == Row2 && item.Row != Row1
                               || Column1 == Column2 && item.Column != Column1
                               || Row1 == Row2 && calcSetList.Where(x=> x.Expend == "no" || x.Expend == "cross" ).Any()
                               || Column1 == Column2 && calcSetList.Where(x => x.Expend == "no" || x.Expend == "portrait").Any())
                            {
                                foreach(var cell in calcList)
                                {
                                    if(cell.Expend == "no")
                                    {
                                        tmp.AddRange(cell.Value.Split(','));
                                    }
                                    else
                                    {
                                        tmp.Add(cell.Value);
                                    }
                                }
                                itemRes.Value = calculatorFunctionValue(tmp.Where(x=>x.IsNotNullOrEmpty()),str);
                            }
                            else
                            {
                                flag = Row1 == Row2;
                                indexList =  calcList.Select(x => flag ? x.Row : x.Column).Distinct();
                                Dictionary<string, string> dic = new();

                                // 此处开始逐行统计
                                foreach(var calcIndex in indexList)
                                {
                                    if (flag)
                                        tmpList = calcList.Where(x => x.Row == calcIndex);
                                    else
                                        tmpList = calcList.Where(x=> x.Column == calcIndex);

                                    foreach(var cell in tmpList)
                                    {
                                        if (flag)
                                            dic[cell.Column.ToString()] = cell.Value;
                                        else
                                            dic[cell.Row.ToString()] = cell.Value;
                                    }

                                    if (flag && itemRes.Row == calcIndex
                                        || !flag && itemRes.Column == calcIndex)
                                    {
                                        itemRes.Value = calculatorFunctionValue(dic.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Value), str);
                                    }
                                    else
                                    {
                                        CellItem  itemTmp = itemRes.DeepClone();
                                        if (flag)
                                            itemTmp.Row = calcIndex;
                                        else
                                            itemTmp.Column = calcIndex;

                                        itemTmp.Value = calculatorFunctionValue(dic.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x=>x.Value),str);
                                        finaAdd.Add(itemTmp);
                                    }
                                }
                            }
                        }
                    }
                }
                //最终计算，将finaAdd 添加进入到 resultCelldata
                resultCelldata.AddRange(finaAdd);
                finaAdd.Clear();
            }
        }
    }
    /// <summary>
    /// 把函数中的结果按照函数计算
    /// </summary>
    private string calculatorFunctionValue(IEnumerable<string> enumerable, string str)
    {
        bool percent = false;
        if(enumerable.Where(x=>x.EndsWith('%')).Any())
        {
            percent = true;
            List<string> list = new List<string>();
            foreach(var item in enumerable)
            {
                if (item.IsNotNullOrEmpty() && item.EndsWith('%'))
                    list.Add(item.Substring(0, item.Length - 1));
                else
                    list.Add(item);
            }
            enumerable = list;
        }
        string res = "";
        switch (str)
        {
            // "=sum(","=count(","=average(","=max(","=min("
            case "=SUM(":
                res = Enumerable.Sum(enumerable.Select(x => double.Parse(x))).ToString();
                break;
            case "=COUNT(":
                res = enumerable.Count().ToString();
                break;
            case "=AVERAGE(":
                res = Math.Round(Enumerable.Average(enumerable.Select(x => double.Parse(x))),6).ToString();
                break;
            case "=MAX(":
                res = Enumerable.Max(enumerable.Select(x => double.Parse(x))).ToString();
                break;
            case "=MIN(":
                res = Enumerable.Min(enumerable.Select(x => double.Parse(x))).ToString();
                break;
            default:
                res = enumerable.Count().ToString();
                break;
        }
        if (percent)
            res = string.Concat(res, '%');
        return res;
    }

    private  (int, int) translateCoordinate(string function)
    {
        int pow = 0;
        int Column = 0;
        string scale = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = function.Length - 1; i >= 0; i--)
        {
            if (!integerCheck(function[i].ToString()))
            {
                Column += (int)Math.Pow(26, pow) * (scale.IndexOf(function[i]) + 1);
                pow++;
            }
        }
        int Row = Convert.ToInt32(function.Substring(pow));
        return (Row - 1, Column - 1);
    }

    /// <summary>
    /// 检查字符串类型
    /// </summary>
    private void checkValueType(List<CellItem> resultCelldata)
    {
        DateTime dateTime;
        DataTable dt;
        List<int> record = new();
        foreach (CellItem cellItem in resultCelldata)
        {
            if(cellItem.SetKey != null && cellItem.SetKey.ToLower().IndexOf("image") != -1)
            {
                if(cellItem.Value.ToLower().IndexOf("https://")!=-1 || cellItem.Value.ToLower().IndexOf("http://") != -1)
                {
                    cellItem.ValueType = "image";
                }else if (cellItem.Value == "System.Byte[]")
                {
                    cellItem.ValueType = "image";
                    dt = getData(new DataCollectInput { SetCode = cellItem.SetCode }).Item1;
                    Byte[] bytes = getByte(dt, cellItem.ReviewValue, cellItem.SetKey, record);
                    cellItem.Value = "data:image/png;base64," + bytes.ToBase64();
                }
            }else if(cellItem.SetKey != null && (cellItem.SetKey.ToLower().IndexOf("date") != -1 || cellItem.SetKey.ToLower().IndexOf("time") != -1))
            {
                if(DateTime.TryParse(cellItem.Value, out dateTime))
                {
                    cellItem.Value = dateTime.ToDateTimeString();
                }
            }
        }
    }
    /// <summary>
    /// 取出图片的二进制数据
    /// </summary>
    private byte[] getByte(DataTable dt, JObject reviewValue,string setKey, List<int> record)
    {
        IEnumerable<DataRow> enums = dt.Select().ToList();
        foreach (var kv in reviewValue)
        {
            enums = enums.Where(x => x[kv.Key].ToString() == kv.Value.ToString());
        }
        if (enums.Any())
        {
            foreach(var item in enums){
                if (record.Contains(dt.Rows.IndexOf(item)))
                {
                    continue;
                }
                else
                {
                    record.Add(dt.Rows.IndexOf(item));
                }
                Object obj = item[setKey];
                if (obj.GetType() == typeof(Byte[]))
                {
                    return (Byte[])obj;
                }
            }
        }
        return new byte[] { };
    }

    /// <summary>
    /// 补充空白单元格
    /// </summary>
    private void replenish(JObject sheetItem, List<CellItem> cellList, List<CellItem> resultCelldata)
    {
        if (sheetItem.SelectToken("blankNum") == null)
            return;

        // 记录已经被占用的格子
        HashSet<string> set = new HashSet<string>();
        // 检查现有的数据占据了多少格子
        foreach (CellItem cellItem in resultCelldata)
        {
            int RowAdd = Math.Max(1, cellItem.RowMerge);
            int colAdd = Math.Max(1, cellItem.ColumnMerge);
            for (int i = 0; i < RowAdd; i++)
            {
                for (int j = 0; j < colAdd; j++)
                {
                    set.Add((cellItem.Row + i) + "-" + (cellItem.Column + j));
                }
            }
        }
        // 获取要填充的最大行
        int blankMaxRow = sheetItem.SelectToken("blankNum").ToInt32();
        // 获取所有的动态单元格
        var list = cellList.Where(x => x.SingleCellType == CellType.DYNAMIC);
        IEnumerable<CellItem> tmpList = new List<CellItem>();
        int max = 0;
        foreach(var item in list)
        {
            if(item.LeftParentRow != -1 && item.TopParentRow != -1)
            {
                tmpList = resultCelldata.Where(x => x.CoordinateRow == item.CoordinateRow && x.CoordinateColumn == item.CoordinateColumn);
                int RowMin = tmpList.Select(x => x.Row).Min();
                int RowMax = tmpList.Select(x => x.Row).Max();
                int colMin = tmpList.Select(x => x.Column).Min();
                int colMax = tmpList.Select(x => x.Column).Max();
                for(; RowMin <= RowMax; RowMin++)
                {
                    for(; colMin <= colMax;colMin++)
                    {
                        if (set.Add(RowMin + "-" + colMin))
                        {
                            var cellTmp = tmpList.First().DeepClone();
                            cellTmp.Row = RowMin;
                            cellTmp.Column = colMin;
                            resultCelldata.Add(cellTmp);
                        }
                    }
                }
            }
            else
            {
                tmpList = resultCelldata.Where(x => x.CoordinateRow == item.CoordinateRow && x.CoordinateColumn == item.CoordinateColumn);
                if (tmpList == null || tmpList.Count() == 0)
                    continue;
                if (item.Expend == "cross")
                {
                    max = tmpList.Select(x => x.Column).Max();
                    var cell = tmpList.First();
                    int count = blankMaxRow - tmpList.Count();
                    for (int i = 1;i<= count; i++)
                    {
                        if (set.Add(cell.Row + "-" + (max + i)))
                        {
                            var cellTmp = cell.DeepClone();
                            cellTmp.Value = "";
                            cellTmp.Column = max + i;
                            resultCelldata.Add(cellTmp);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }else if(item.Expend == "portrait")
                {
                    max = tmpList.Select(x => x.Row).Max();
                    var cell = tmpList.First();
                    int count = blankMaxRow - tmpList.Count();
                    for (int i = 1; i <= count; i++)
                    {
                        if (set.Add((max + i) + "-" + cell.Column))
                        {
                            var cellTmp = cell.DeepClone();
                            cellTmp.Value = "";
                            cellTmp.Row = max + i;
                            resultCelldata.Add(cellTmp);
                        }
                        else
                        {
                            continue;
                        }
                    }
                } 
            }
        }
    }
    
    /// <summary>
    /// 优先加载数据源
    /// </summary>
    private void loadingData(JObject sheetItem,List<CellItem> cellList, string setParam, int requestCount, int pageSize,bool export,string exportType)
    {
        // 获取所有的数据集
        var setList = cellList.Select(x => x.SetCode).Distinct();
        // 获取JS验证工具类
        var engine = getResolver();
        foreach (var setCode in setList)
        {
            if (setCode == null)
                continue;
            // 开始加载数据集的数据（此处应该根据实际情况自动判断需不需要加载全部的数据）
            // 1，此处先加载查询参数(默认每三页一千条数据，作为备选，以便筛选)
            DataCollectInput collectInput = getDataSet(setCode, setParam, requestCount, pageSize);
            collectInput.Export = export;
            collectInput.IsPreview = true;
            // 获取数据集信息（后面用的太多）
            DataCollect dataCollect = (repository.Queryable<DataCollect>().Where(x => x.SetCode == setCode).ToList()).First();

            // 2，此处确定排序字段
            // 获取所有的最父格keyList
            var keyList = cellList.Where(x => x.SetCode == setCode);

            List<CellItem> fList = keyList.Where(x=>x.ExpendSort.IsNotNullOrEmpty() && x.ExpendSort != "no").OrderBy(x => x.Row).ThenBy(x => x.Column).ToList();
            collectInput.OrderArr = fList.Select(x => x.SetKey+"-"+x.ExpendSort).Distinct().ToList();
            // 3，此处确定是否需要查询所有的数据
            var (dt, pageCount, total) = (new DataTable(),0L,0L);
            var sourceType = (repository.Queryable<DataSource>().Where(x => x.SourceCode == dataCollect.SourceCode).ToList()).FirstOrDefault().SourceType;
            if (export || "http" == sourceType)
            {
                collectInput.SearchAll = true;
                (dt, pageCount, total) = getData(collectInput);
            }
            else 
            {
                // 过滤数据转sql
                var filtration =  keyList.Where(x => x.FinalFather && !string.IsNullOrEmpty(x.FilterData));
                if (filtration.Any())
                {
                    collectInput.SqlFiltration = getSqlFiltration(dataCollect, filtration);
                }

                // 汇总转sql
                filtration = keyList.Where(x => x.ShowType == "summary" );
                foreach (var fItem in filtration)
                {
                    List<CellItem> cellItems = getAllFCell(keyList, fItem);
                    if(cellItems.Count == cellItems.Where(x=>x.ShowType == "group").Count()){
                        BaseErrorCode.SummaryFlag = true;
                    }
                }
                if (BaseErrorCode.SummaryFlag && filtration.Any())
                {
                    collectInput.SearchAll = true;
                    string sqlFiltrationRecord = collectInput.SqlFiltration;
                    foreach (var fItem in filtration)
                    {
                        // 获取所有父格的设定值
                        if(collectInput.GroupList.Count != 0)
                        {
                            List<string> groupListTmp = getAllFCell(keyList, fItem).Select(x => x.SetKey).ToList();
                            if(groupListTmp.Count+2 != collectInput.GroupList.Count || !collectInput.GroupList.ContainsAll(groupListTmp.ToArray()))
                            {
                                throw new Exception("汇总单元格父格信息不一致！请重新设定！！");
                            }
                        }
                        else
                        {
                            collectInput.GroupList = getAllFCell(keyList, fItem).Select(x=>x.SetKey).ToList();
                            // 添加自身用于别名
                            collectInput.GroupList.Add(fItem.SetKey);
                            collectInput.GroupList.Add(fItem.ShowValue);
                        }
                        if (!string.IsNullOrEmpty(fItem.FilterData))
                        {
                            collectInput.SqlFiltration = getSqlFiltration(dataCollect, null, sqlFiltrationRecord, fItem);
                        }
                        if(dt.Rows.Count > 0)
                        {
                            removeData(new DataCollectInput { SetCode = setCode });
                            var (dtN, pageCountN, totalN) = getData(collectInput);
                            // 合并两个dt信息
                            mergeDataTable(dt, dtN, collectInput.GroupList, fItem);
                            fItem.SetKey = fItem.SetKey + fItem.Row + fItem.Column;
                            //appendTargetColumns(dt, dataCollect);
                        }
                        else
                        {
                            (dt, pageCount, total) = getData(collectInput);
                            // DataTable 补充缺少的列信息
                            appendTargetColumns(dt, dataCollect);
                        }
                    }
                }
                else
                {
                    (dt, pageCount, total) = getData(collectInput);
                }
            }
            // 6，去重 并做 数据筛选
            DataTable dtResult = dt.Clone();
            // 解锁 DataTable 
            unLockReadOnly(dtResult);
            unLockReadOnly(dt);
            if (collectInput.SearchAll && "http" != sourceType)
            {
                // 筛选处理方式 
                if(keyList.Where(x => x.FilterData.IsNotNullOrEmpty() && !x.FinalFather && x.ShowType != "summary").Any())
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (checkRow(keyList, dt, dtResult, dt.Rows[i], i == dt.Rows.Count-1, engine))
                        {
                            //dtResult.ImportRow(dt.Rows[i]);
                            dtResult.Rows.Add(dt.Rows[i].ItemArray);
                        }
                    }
                    dt = dtResult;
                }

                // 不同情况不同处理
                if (!export){ dt = getRange((requestCount - 1) * pageSize, requestCount * pageSize, dt); }
            }
            else if(keyList.Where(x => x.ShowType == "list").Any())
            {
                if (exportType == "3" && dt.Rows.Count > 10000)
                {
                    throw new Exception("数据量大于一万禁止导出");
                }
                if (pageSize == int.MaxValue)
                {
                    dtResult = dt;
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (checkRow(keyList, dt, dtResult, dt.Rows[i], i == dt.Rows.Count - 1, engine))
                        {
                            dtResult.Rows.Add(dt.Rows[i].ItemArray);
                        }
                        if (dtResult.Rows.Count >= pageSize)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                /*暂时保留去重方法
                 * DataView dv = new DataView(dt);
                dt = dv.ToTable(true, GetTableColumnName(dt));*/
                if(exportType == "3" && dt.Rows.Count > 10000)
                {
                    throw new Exception("数据量大于一万禁止导出");
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if ( checkRow(keyList,dt , dtResult, dt.Rows[i], i == dt.Rows.Count-1, engine))
                    {
                        dtResult.Rows.Add(dt.Rows[i].ItemArray);
                    }
                    if(dtResult.Rows.Count >= pageSize)
                    {
                        break;
                    }
                }
            }
            // 分页信息
            if(total == -1)
            {
                if (sheetItem["pageCount"] == null)
                    sheetItem.Add("pageCount", -1);
                if (sheetItem["total"] == null)
                    sheetItem.Add("total", -1);
            }else if ( total != -1 &&(dtResult.Rows.Count > 0 || dt.Rows.Count > 0))
            {
                if (sheetItem["pageCount"] == null)
                    sheetItem.Add("pageCount", total/pageSize+1);
                if (sheetItem["total"] == null)
                    sheetItem.Add("total", total);
            }
            // 7，进行函数值运算(此处应该根据全部的查询值就行运算，当计算有函数参与的时候searchAll == true)
            // 8，修改结果值缓存

            //dtResult = getRange((requestCount - 1) * pageSize, dtResult.Rows.Count, dtResult);

            engine.Dispose();
            keyValuePairs[setCode] = (collectInput.SearchAll? dt:dtResult, total % pageSize == 0 ? total / pageSize : (total / pageSize) + 1, total);
        }
    }
    /// <summary>
    /// 获取dataTable列名的数组
    /// </summary>
    private  string[] GetTableColumnName(DataTable dt)
    {
        StringBuilder cols = new();
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            cols .Append (dt.Columns[i].ColumnName );
            cols.Append(",");
        }
        return cols.ToString().TrimEnd(',').Split(',');
    }

    /// <summary>
    /// 解锁DataTable 字段只读状态
    /// </summary>
    /// <param name="dtResult"></param>
    private void unLockReadOnly(DataTable dtResult)
    {
        for (int i = 0; i < dtResult.Columns.Count; i++)
        {
            dtResult.Columns[dtResult.Columns[i].ColumnName].ReadOnly = false;
        }
    }

    /// <summary>
    /// 根据setcode 获取sql的全部列信息，添加到DataTable dt中
    /// </summary>
    private void appendTargetColumns(DataTable dt, DataCollect dataCollect)
    {
        List<string> list = dataCollectServices.getSetParamList(dataCollect.CaseResult, dataCollect.SetType, dataCollect.SetDesc);
        foreach(var item in list)
        {
            if (!dt.Columns.Contains(item))
            {
                dt.Columns.Add(item);
            }
        }
    }

    /// <summary>
    /// 根据groupList将dtN 合并到 dt 中，并根据setCode补充 Columns
    /// </summary>
    private void mergeDataTable(DataTable dt, DataTable dtN, List<string> groupList,CellItem fItem)
    {
        string newKey = fItem.SetKey + fItem.Row + fItem.Column;
        dt.Columns.Add(newKey);

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            List<DataRow> enums = dtN.Select().ToList();
            for (int j = 0; j< groupList.Count;j++)
            {
                if(j == groupList.Count - 2)
                {
                    if (enums.Count == 0)
                        dt.Rows[i][newKey] = 0;
                    else
                        dt.Rows[i][newKey] = enums.First()[fItem.SetKey];
                    break;
                }
                var str = dt.Rows[i][groupList[j]].ToString();
                enums = enums.Where(x => x[groupList[j]].ToString() == dt.Rows[i][groupList[j]].ToString()).ToList();
            }
        }
    }

    /// <summary>
    /// 穷举单元格的所有父格的setKey
    /// </summary>
    private List<CellItem> getAllFCell(IEnumerable<CellItem> keyList, CellItem fItem)
    {
        List<CellItem> resList = new();
        List<CellItem> list = new();
        list.Add(fItem);
        List<CellItem> tmpList = new();

        while (list.Any())
        {
            tmpList.Clear();
            foreach (CellItem item in list)
            {
                if(item.LeftParentRow != -1)
                {
                    tmpList.AddRange(keyList.Where(x=>x.Row == item.LeftParentRow && x.Column == item.LeftParentColumn));
                }
                if(item.TopParentRow != -1)
                {
                    tmpList.AddRange(keyList.Where(x => x.Row == item.TopParentRow && x.Column == item.TopParentColumn));
                }
            }
            foreach(var item in tmpList)
            {
                resList.Add(item);
            }
            list.Clear();
            list.AddRange(tmpList);
        }
        return resList;
    }

    /// <summary>
    /// 最父格筛选转化为sql条件
    /// </summary>
    private string getSqlFiltration(DataCollect dataCollect, IEnumerable<CellItem> filtration,string sqlFiltration = "", CellItem cellItem = null)
    {
        // 此处要讲最父格的筛选转换为sql,多个父格的筛选条件 & 连接

        if(cellItem != null && filtration == null)
        {
            List<CellItem>  filtration2 = new List<CellItem>();
            filtration2.Add(cellItem);
            filtration = filtration2;
        }

        // 获取数据源类型
        string sourceType = (repository.Queryable<DataSource>().Where(x =>
            x.SourceCode == dataCollect.SourceCode
        ).ToList()).First().SourceType;

        foreach (var item in filtration)
        {
            if (item.FilterData.IsNullOrEmpty())
                continue;
            //1,替换符号 ===  =>  =      !==  =>  <>  
            item.FilterData = item.FilterData.Replace("===", "=").Replace("!==", "<>");
            //2,替换双引号 "  =>  '
            item.FilterData = item.FilterData.Replace("\"","'");
            //3,替换indexOf   ( WORKORDER ).indexOf(  =>  -1+instr(WORKORDER,
            if (item.FilterData.IndexOf(".indexOf") != -1)
            {
                StringBuilder sb = new StringBuilder();
                var list = dataCollectServices.getSetParamList(dataCollect.CaseResult, dataCollect.SetType, dataCollect.SetDesc);
                int replaceTmp = 0;
                foreach (var str in list)
                {
                    sb.Clear();
                    sb.Append("( ");
                    sb.Append(str);
                    sb.Append(" )");
                    if (item.FilterData.IndexOf(sb+ ".indexOf(") != -1)
                    {
                        item.FilterData = item.FilterData.Replace(sb + ".indexOf(", $"-1+instr({str},");
                    }
                    string tmp = ".indexOf(" + sb;
                    if (item.FilterData.IndexOf(tmp) != -1)
                    {
                        List<int> replaceTmpList = new List<int>();
                        while ((replaceTmp = item.FilterData.IndexOf(tmp, replaceTmp))!=-1)
                        {
                            replaceTmpList.Add(replaceTmp);
                            replaceTmp += tmp.Length;
                        }
                        item.FilterData = item.FilterData.Replace(tmp, ","+str);

                        //获取当前判断条件的开始index （获取and 的index）   eg  and "0110014519398".indexOf(( WORKORDER )) !==-1 
                        foreach(var tmpIndex in replaceTmpList)
                        {
                            int index = getBeforeIndex(item.FilterData, tmpIndex);
                            item.FilterData = item.FilterData.Substring(0, index) + " -1+instr(" + item.FilterData.Substring(index);
                        }
                    }

                }
            }
            if (string.IsNullOrEmpty(sqlFiltration))
            {
                sqlFiltration = item.FilterData;
            }
            else
            {
                sqlFiltration = "( " + sqlFiltration + " ) and ( "+  item.FilterData +" )";
            }
        }
        return sqlFiltration;
    }
    /// <summary>
    /// 获取当前判断条件的开始index
    /// </summary>
    private int getBeforeIndex(string filterData, int replaceTmp)
    {
        int index = -1;
        string subStr = filterData.Substring(0, replaceTmp);
        //去掉回车换行符
        string[] arr = {" ( "," and "," or " };
        foreach(var item in arr)
        {
            index = Math.Max(index, subStr.LastIndexOf(item)==-1?-1: subStr.LastIndexOf(item)+item.Length);
        }
        return index == -1 ? 0: index;
    }

    /// <summary>
    /// 获取DataTable从第startRow行到endRow行的数据
    /// </summary>
    private DataTable getRange(int startRow, int endRow, DataTable dtResult)
    {
        DataTable result = dtResult.Clone();
        for(int i = 0;i< dtResult.Rows.Count; i++)
        {
            if (i >= startRow && i < endRow)
            {
                result.Rows.Add(dtResult.Rows[i].ItemArray);
            }
        }
        return result;
    }

    /// <summary>
    /// 检验当前条数据是否父格标准
    /// </summary>
    private bool checkRow(IEnumerable<CellItem> keyList, DataTable dt, DataTable  dtResult, DataRow dtRow,bool endFlag, V8ScriptEngine engine)
    {
        StringBuilder sb = new StringBuilder();
        // 遍历所有需要做过滤的字段
        var filterList = keyList.Where(x => x.FilterData != null && !x.FinalFather && x.ShowType != "summary");
        foreach (var item in filterList)
        {
            if (item.FilterData.IsNullOrEmpty())
                continue;
            string filterStr = item.FilterData;
            // 将设定的过滤字段替换成实际的值
            string[] arr = getColumnsByDataTable(dt);
            foreach (var key in arr)
            {
                sb.Clear();
                sb.Append("( ");
                sb.Append(key);
                sb.Append(" )");
                if (filterStr.IndexOf(sb.ToString()) != -1)
                {
                    filterStr = filterStr.Replace(sb.ToString(), "\""+ dtRow[key]+ "\"");
                }
            }
            filterStr = filterStr.Replace("and", "&&").Replace("or", "||");
            var newKey = item.SetKey + item.Row + item.Column; 
            if (!dt.Columns.Contains(newKey))
            {
                dt.Columns.Add(newKey);
                dtResult.Columns.Add(newKey);
            }
            if (!getExpressionResult(filterStr, engine))
            {
                if (item.FinalFather)
                {
                    return false;
                }
            }
            else
            {
                dtRow[newKey] = dtRow[item.SetKey];
            }
            if (endFlag)
            {
                item.SetKey = newKey;
            }
        }
        return true;
    }
    /// <summary>
    /// 获取所有列名
    /// </summary>
    private string[] getColumnsByDataTable(DataTable dt)
    {
        string[] strColumns = null;


        if (dt.Columns.Count > 0)
        {
            int ColumnNum = 0;
            ColumnNum = dt.Columns.Count;
            strColumns = new string[ColumnNum];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                strColumns[i] = dt.Columns[i].ColumnName;
            }
        }


        return strColumns;
    }

    /// <summary>
    /// 合并所有的单元格
    /// </summary>
    private void mergeCellAll(List<CellItem> resultCelldata, Dictionary<string, int> indexRecord)
    {
        // init  record 来合并单元格 for 行 ， for 列
        // (后期可以考虑改进虹吸原理来合并单元格)
        // 统计所有的行或者列的最大拓展行或者列
        Dictionary<string, int> dic = new Dictionary<string, int>();
        string minKey;
        string maxKey;
        foreach (var item in resultCelldata)
        {
            minKey = item.CoordinateRow + "RowMin";
            maxKey = item.CoordinateRow + "RowMax";
            dic[minKey] = Math.Min(dic.GetOrAdd(minKey, item.Row), item.Row);
            dic[maxKey] = Math.Max(dic.GetOrAdd(maxKey, item.Row), item.Row);
            minKey = item.CoordinateColumn + "colMin";
            maxKey = item.CoordinateColumn + "colMax";
            dic[minKey] = Math.Min(dic.GetOrAdd(minKey, item.Column), item.Column);
            dic[maxKey] = Math.Max(dic.GetOrAdd(maxKey, item.Column), item.Column);
        }
        // 开始合并单元格
        int maxIndex;
        string maxIndexKey;
        Boolean flag;
        foreach (var item in resultCelldata)
        {
            
            if (item.ShowType != "group") continue;
            // 计算行
            maxIndexKey = "RowInit0";
            maxIndex = 0;
            // 判断子格是否在同行
            flag = false;
            flag =  resultCelldata.Where(x => x.Row == item.Row
                                    && x.LeftParentRow == item.CoordinateRow
                                    && x.LeftParentColumn == item.CoordinateColumn
                                    || x.Row == item.Row
                                    && x.TopParentRow == item.CoordinateRow
                                    && x.TopParentColumn == item.CoordinateColumn).Any();
            // 1,根据同一原始行向下合并
            int max = dic[item.CoordinateRow + "RowMax"] + 1;
            var tmp = resultCelldata.Where(x => x.Column == item.Column && x.Row > item.Row && x.Row < max);
            foreach (var t in tmp)
            {
                max = Math.Min(max, t.Row);
            }
            // 2,根据indexRecord中的Record 记录来限制最大合并行
            foreach(var kv in indexRecord)
            {
                if(kv.Key.IndexOf("Row") != -1 && kv.Key.IndexOf("Init") != -1 && kv.Value <= item.Row)
                {
                    if(maxIndex < kv.Value)
                    {
                        maxIndex = kv.Value;
                        maxIndexKey = kv.Key;
                    }
                }
            }
            // 3,计算indexRecord中的最大合并行
            int restrict = indexRecord.GetOrAdd(maxIndexKey.Replace("Init", "Record"), 1) - (item.Row - maxIndex);
            if (flag)
            {
                item.RowMerge = Math.Max(item.RowMerge, (max - item.Row) <= 0 ? 1 : (max - item.Row));
            }
            else
            {
                item.RowMerge = Math.Max(item.RowMerge, Math.Min((max - item.Row) <= 0 ? 1 : (max - item.Row), restrict <= 0 ? 1 : restrict));
            }
            
            
            // 计算列
            maxIndexKey = "ColumnInit0";
            maxIndex = 0;
            // 判断子格是否在同列
            flag = false;
            flag = resultCelldata.Where(x => x.Column == item.Column
                                   && x.LeftParentRow == item.CoordinateRow
                                   && x.LeftParentColumn == item.CoordinateColumn
                                   || x.Column == item.Column
                                   && x.TopParentRow == item.CoordinateRow
                                   && x.TopParentColumn == item.CoordinateColumn).Any();
            // 1,根据同一原始列向右合并
            max = dic[item.CoordinateColumn + "colMax"] + 1;
            tmp = resultCelldata.Where(x => x.Row == item.Row && x.Column > item.Column && x.Column < max);
            foreach (var t in tmp)
            {
                max = Math.Min(max, t.Column);
            }
            // 2,根据indexRecord中的Record 记录来限制最大合并列
            foreach (var kv in indexRecord)
            {
                if (kv.Key.IndexOf("Column") != -1 && kv.Key.IndexOf("Init") != -1 && kv.Value <= item.Column)
                {
                    if (maxIndex < kv.Value)
                    {
                        maxIndex = kv.Value;
                        maxIndexKey = kv.Key;
                    }
                }
            }
            // 3,计算indexRecord中的最大合并列
            int restrict2 = indexRecord.GetOrAdd(maxIndexKey.Replace("Init", "Record"), 1) - (item.Column - maxIndex);
            if (flag)
            {
                item.ColumnMerge = Math.Max(item.ColumnMerge,(max - item.Column) <= 0 ? 1 : (max - item.Column));
            }
            else
            {
                item.ColumnMerge = Math.Max(item.ColumnMerge, Math.Min((max - item.Column) <= 0 ? 1 : (max - item.Column), restrict2 <= 0 ? 1 : restrict2));
            }
        }
    }

    // 最后再样式克隆
    private void styleCloneAll(JObject sheetItem, List<CellItem> resultCelldata, Dictionary<string, int> indexRecord)
    {
        foreach (CellItem cellItem in resultCelldata)
        {
            styleClone(sheetItem, false, cellItem.CoordinateRow, cellItem.CoordinateColumn, cellItem.Row, cellItem.Column);
        }
    }

    /// <summary>
    /// 开始加载单元格对象
    /// </summary>
    public void analysisCellData(JArray relationList,List<CellItem> cellList, CellItem cellItem, 
        List<CellItem> resultCelldata,Dictionary<string, int> indexRecord)
    {
        // 块的定位
        string[] arr;
        //获取行号
        int cellR = cellItem.Row;
        //获取列数
        int cellC = cellItem.Column;
        // 当前单元格 是否在relationList的行或列中
        JArray blockList = new JArray();
        // 筛选在块的行或者列中的单元格
        foreach (JObject relation in relationList)
        {
            arr = ((string)relation["start"]).Split(',');
            int Row = arr[0].ToInt32();
            int Column = arr[1].ToInt32();
            if (cellC >= Column || cellR >= Row)
            {
                arr = ((string)relation["end"]).Split(',');
                if ((cellC >= Column && cellC <= arr[1].ToInt32()) || (cellR >= Row && cellR <= arr[0].ToInt32()))
                {
                    blockList.Add(relation);
                }
            }
        }
        // 不在块的行或者列中
        if (blockList.Count == 0)
        {
            cellLoading(indexRecord, resultCelldata, cellItem);
        }
        else
        {
            Boolean flag = false;
            foreach (JObject relation in blockList)
            {
                arr = ((string)relation["start"]).Split(',');
                int Row = arr[0].ToInt32();
                int Column = arr[1].ToInt32();
                if (cellC >= Column && cellR >= Row)
                {
                    arr = ((string)relation["end"]).Split(',');
                    if (cellC >= Column && cellC <= arr[1].ToInt32() && cellR >= Row && cellR <= arr[0].ToInt32())
                    {
                        flag = true;
                        inCellLoading(cellList,relation, indexRecord, resultCelldata, cellItem);
                        break;
                    }
                }
            }
            if (!flag) // 在块外面，在行和列中
            {
                outerCellLoading(cellList, blockList, indexRecord,resultCelldata, cellItem);
            }
        }
    }
    /// <summary>
    /// 加载块内的数据
    /// </summary>
    private void inCellLoading(List<CellItem> cellList, JObject relation, Dictionary<string, int> indexRecord,List<CellItem> resultCelldata, CellItem cellItem)
    {

        string[] arr1 = relation["start"].ToString().Split(',');
        string[] arr2 = relation["end"].ToString().Split(',');
        // 1，获取块中所有单元格
        var tmp = getBlockItems(cellList, arr1[0], arr1[1], arr2[0], arr2[1]);
        // 2，获取最父格
        var res = tmp.Where(x => x.FinalFather);
        // 3，排序
        List<CellItem> fArray = res.OrderBy(x => x.Row).ThenBy(x => x.Column).ToList();
        // 优先加载块内外跟块无关的元素 
        // 获取所有的单元格数据 
        // 4.1，优先遍历无关数据 
        for (int i = 0; i < fArray.Count; i++)
        {
            if (indexRecord.ContainsKey(fArray[i].Row + "-" + fArray[i].Column)) { continue; }
            // 获取块中单元格循环直径
            int[] diameter = getCellCircInfo(tmp, fArray[i]);
                
            // 获取当前父格的子格
            var chilList = tmp.Where(x => x.LeftParentRow == fArray[i].Row && x.LeftParentColumn == fArray[i].Column
                                    || x.TopParentRow == fArray[i].Row && x.TopParentColumn == fArray[i].Column);
            while (chilList.Any())
            {
                foreach (var chil in chilList)
                {
                    #region 计算跟块无关的内容
                    // 计算方块内外的跟父子格无关的动态值
                    if (fArray[i].Expend == "cross")
                    {
                        int begin = fArray[i].Column;
                        int end = fArray[i].Column;
                        if(chil.Column > end)
                        {
                            end = chil.Column;
                        }else if(chil.Column < begin)
                        {
                            begin = chil.Column;
                        }
                        // 查找  begin < 列数 < end 的所有没有父子关系的单元格
                        var dynamic = cellList.Where(x => x.Column >= begin && x.Column <= end
                                    && x.Expend == "cross"
                                    && x.LeftParentRow == -1 && x.TopParentRow == -1
                                    );

                        foreach (var single in dynamic)
                        {
                            // 判断当前单元格有无加载过
                            if (indexRecord.ContainsKey(single.SetValue)) { continue; }
                            // 判断为当前块的最父单元格
                            if (fArray.Where(x=> x.Row == single.Row && x.Column == single.Column).Any()) { continue; }
                            // 判断当前单元格是否是父格
                            if(cellList.Where(x=> x.LeftParentColumn == single.Column && x.LeftParentRow == single.Row
                                                 ||x.TopParentRow == single.Row && x.TopParentColumn == single.Column).Any()) { continue; }
                            // 获取single的值
                            var (sarray, spageCount, stotal) = getData(new DataCollectInput { SetCode = single.SetCode});
                            var sarr = sarray.Select().Select(x => x[single.SetKey]?.ToString()).ToList();
                            //var sarr = sarray.Select(x => (string)x[single.SetKey]).ToList();
                            var (sRow,scol) = getIndex(indexRecord, single.Row, single.Column);
                            // 此处对 sarr 进行特殊处理
                            sarr = calcShowType(single.ShowType,single.ShowValue, sarr);
                                
                            for (int k = 0; k < sarr.Count; k++)
                            {
                                var singleTmp = single.DeepClone();
                                singleTmp.Row = sRow;
                                singleTmp.Column = scol + diameter[1] * k;
                                singleTmp.Value = sarr[k];
                                resultCelldata.Add(singleTmp);
                                // styleClone(sheetItem,false,single.Row,single.Column,sRow, scol + diameter[1] * k);
                            }
                        }
                    }
                    else if (fArray[i].Expend == "portrait")
                    {
                        int begin = fArray[i].Row;
                        int end = fArray[i].Row;
                        if (chil.Row > end)
                        {
                            end = chil.Row;
                        }
                        else if (chil.Row < begin)
                        {
                            begin = chil.Row;
                        }
                        // 查找  begin < 列数 < end 的所有没有父子关系的单元格
                        var dynamic = cellList.Where(x => x.Row >= begin && x.Row <= end
                                    && x.Expend == "portrait"
                                    && x.LeftParentRow == -1 && x.TopParentRow == -1
                                    );

                        foreach (var single in dynamic)
                        {
                            // 判断有无已经加载过的单元格
                            if (indexRecord.ContainsKey(single.Row + "-" + single.Column)) { continue; }
                            // 判断为当前块的最父单元格
                            if(fArray.Where(x=>x.Row == single.Row && x.Column == single.Column).Any()) { continue; }
                            // 判断当前单元格是否是父格
                            if (cellList.Where(x => x.LeftParentColumn == single.Column && x.LeftParentRow == single.Row
                                                 || x.TopParentRow == single.Row && x.TopParentColumn == single.Column).Any()) { continue; }
                            // 记录 single的值
                            indexRecord[single.Row + "-" + single.Column] = 1;
                            // 获取single的值
                            var (sarray, spageCount, stotal) = getData(new DataCollectInput { SetCode = single.SetCode});
                            var sarr = sarray.Select().Select(x => x[single.SetKey]?.ToString()).ToList();
                            // var sarr = sarray.Select(x => (string)x[single.SetKey]).ToList();
                            var (sRow, scol) = getIndex(indexRecord, single.Row, single.Column);
                            sarr = calcShowType(single.ShowType, single.ShowValue, sarr);
                            for (int k = 0; k < sarr.Count; k++)
                            {
                                var singleTmp = single.DeepClone();
                                singleTmp.Row = sRow + diameter[0] * k;
                                singleTmp.Column = scol;
                                singleTmp.Value = sarr[k];
                                resultCelldata.Add(singleTmp);
                                // styleClone(sheetItem, false, single.Row, single.Column, sRow + diameter[0] * k, scol);
                            }
                        }
                    }
                    // 计算方块内外的跟父子格无关的静态值
                    #endregion
                    // 此处根据条件判断是否跟随筛选（先把逻辑铺垫好，等待后续实现）
                    if(chil.LeftParentRow != -1 
                        && chil.LeftParentRow == fArray[i].CoordinateRow
                        && chil.LeftParentColumn == fArray[i].CoordinateColumn)
                    {
                        // 此处取行对应的静态单元格
                        int begin = fArray[i].Row;
                        int end = fArray[i].Row;
                        if (chil.Row > end)
                        {
                            end = chil.Row;
                        }
                        else if (chil.Row < begin)
                        {
                            begin = chil.Row;
                        }
                        var staticList = cellList.Where(x => string.IsNullOrEmpty(x.Expend)
                                && x.Row >= begin && x.Row <= end
                                && x.Row != fArray[i].Row
                                || string.IsNullOrEmpty(x.Expend) && x.Row == fArray[i].Row && x.Column > fArray[i].Column
                                );
                        
                        foreach (var x in staticList)
                        {
                            if (!indexRecord.TryAdd(x.Row + "-" + x.Column, 1))
                                continue;
                            var (sRow,scol) = getIndex(indexRecord, x.Row, x.Column);
                            var clone = x.DeepClone();
                            clone.Row = sRow;
                            clone.Column = scol;
                            resultCelldata.Add(clone);
                                
                        }
                    }
                    else if(chil.TopParentRow != -1
                        && chil.TopParentRow == fArray[i].CoordinateRow
                        && chil.TopParentColumn == fArray[i].CoordinateColumn)
                    {
                        // 此处取列对应的静态单元格
                        int begin = fArray[i].Column;
                        int end = fArray[i].Column;
                        if (chil.Column > end)
                        {
                            end = chil.Column;
                        }
                        else if (chil.Column < begin)
                        {
                            begin = chil.Column;
                        }
                        var staticList = cellList.Where(x => string.IsNullOrEmpty(x.Expend)
                                && x.Column != fArray[i].Column
                                && x.Column >= begin && x.Column <= end
                                || string.IsNullOrEmpty(x.Expend) && x.Column == fArray[i].Column && x.Row > fArray[i].Row);
                        foreach (var x in staticList)
                        {
                            var (sRow, scol) = getIndex(indexRecord, x.Row, x.Column);
                            var clone = x.DeepClone();
                            clone.Row = sRow;
                            clone.Column = scol;
                            resultCelldata.Add(clone);
                        }
                    }
                }
                List<CellItem> tmpChil = new List<CellItem>();
                foreach (var chil in chilList) {
                    tmpChil.AddRange(tmp.Where(x => x.LeftParentRow == chil.Row && x.LeftParentColumn == chil.Column
                                || x.TopParentRow == chil.Row && x.TopParentColumn == chil.Column));
                }
                chilList = tmpChil;

            }
        }
        // 4.2，循环遍历最父格
        for (int i = 0; i < fArray.Count; i++)
        {
            if (indexRecord.ContainsKey(fArray[i].Row + "-" + fArray[i].Column)) { continue; }
            var (array, pageCount, total) = getData(new DataCollectInput { SetCode = fArray[i].SetCode});
            if(array == null || array.Rows.Count <= 0) { continue; }
            var sarr = array.Select().Select(x => x[fArray[i].SetKey]?.ToString()).ToList();
            //var sarr = array.Select(x => (string)x[fArray[i].SetKey]).ToList();
            sarr = calcShowType(fArray[i].ShowType, fArray[i].ShowValue, sarr);
            int circularNum = sarr.Count;
            // 获取块中单元格循环直径

            int[] diameter = getCellCircInfo( tmp, fArray[i]);
            // 开始填写父格的值
            indexRecord[fArray[i].Row + "-" + fArray[i].Column] = 1;
            // var (Row1, col1) = getIndex(indexRecord, fArray[i].Row, fArray[i].Column);
            var (Row1, col1) = getIndex(indexRecord,  fArray[i].Row, fArray[i].Column);
            switch (fArray[i].Expend)
            {
                case "no":
                    #region 不拓展
                    StringBuilder sb = new StringBuilder();
                    foreach (var s in sarr)
                    {
                        sb.Append(s);
                        sb.Append(",");
                    } 
                    sb = sb.Remove(sb.Length - 1, 1);
                    var fTmp = fArray[i].DeepClone();
                    fTmp.Row = Row1;
                    fTmp.Column = col1;
                    fTmp.Value = sb.ToString();
                    fTmp.ReviewValue = new JObject(new JProperty(fArray[i].SetKey, sb.ToString()));
                    resultCelldata.Add(fTmp);
                    // styleClone(sheetItem, true, fArray[i].Row, fArray[i].Column, Row1, col1);
                    break;
                #endregion
                case "cross":
                    #region 横向拓展
                    // 1,判断父子格之间是否有交集
                    // 1,获取循环的最后一列
                    int Column = fArray[i].Column + diameter[3]; 
                    // 2,获取循环最后一列拓展数量(待添加循环比较)
                    int record = getRecord(indexRecord, "Column", Column, -1);
                    // 3,将 resultCellData 中的 targetCol 列后的数据向后推
                    pushBack(resultCelldata,"Column", Column,-1, (sarr.Count-1)* diameter[1]- record+1);
                    // 4,修改indexRecord中的记录，将列大于 Column 的记录加上当前父格的拓展-1
                    changeIndex(indexRecord, "Column", Column, -1, (sarr.Count - 1) * diameter[1] - record + 1);
                    // 5,开始填值
                    for (int j = 0; j < sarr.Count; j++)
                    {
                        // 父格肯定只加载一个，所以Record 应该只记录一个
                        getRecord(indexRecord, "Column", fArray[i].Column, col1 + j * diameter[1]);
                        var fTmp2 = fArray[i].DeepClone();
                        fTmp2.Row = Row1;
                        fTmp2.Column = col1 + j * diameter[1];
                        fTmp2.Value = sarr[j];
                        fTmp2.ReviewValue = new JObject(new JProperty(fArray[i].SetKey, sarr[j]));
                        resultCelldata.Add(fTmp2);
                        // styleClone(sheetItem, false, fArray[i].Row, fArray[i].Column, Row1, col1 + j * diameter[1]);
                    }
                    #region 旧逻辑 (注释)
                    /*int expendCount = indexRecord.GetOrAdd("ColumnRecord" + arr2[1],1);
                    int expendNum = (sarr.Count - 1) * diameter[1];
                    int num = (expendNum+1 - expendCount) <= 0 ? 0 : (expendNum+1 - expendCount);
                    if (num > 0)
                    { // 将大于等于当前列的拓展记录 + num
                        indexRecord["ColumnRecord" + arr2[1]] = sarr.Count();
                        changeIndex(indexRecord, "Column", Convert.ToInt32(arr2[1]), -1, num);
                    }
                    for(int j = 0; j < sarr.Count; j++)
                    {
                        var fTmp2 = fArray[i].DeepClone();
                        fTmp2.Row = Row1;
                        fTmp2.Column = col1 + j* diameter[1];
                        fTmp2.Value = sarr[j];
                        fTmp2.ReviewValue = new JObject(new JProperty(collectInput.SetDesc, sarr[j]));
                        resultCelldata.Add(fTmp2);
                        styleClone(sheetItem, false, fArray[i].Row, fArray[i].Column, Row1, col1 + j * diameter[1]);
                    }*/
                    #endregion
                    break;
                #endregion
                case "portrait":
                    #region 纵向拓展，把大于块最后一行的单元格往下推
                    // 1,获取循环的最后一行
                    int Row = fArray[i].Row + diameter[2];
                    // 2,获取循环最后一行拓展数量(待添加循环比较)
                    int record2 = getRecord(indexRecord, "Row", Row, -1);
                    // 3,将 resultCellData 中的 targetRow 行后的数据向后推
                    pushBack(resultCelldata, "Row", Row,-1, (sarr.Count - 1) * diameter[0] - record2 + 1);
                    // 4,修改indexRecord中的记录，将列大于 Column 的记录加上当前父格的拓展-1
                    changeIndex(indexRecord, "Row", Row, -1, (sarr.Count - 1) * diameter[0] - record2 + 1);
                    // 5,开始填值
                    for (int j = 0; j < sarr.Count; j++)
                    {
                        // 父格肯定只加载一个，所以Record 应该只记录一个
                        getRecord(indexRecord, "Row", fArray[i].Row, Row1 + j * diameter[0]);
                        var fTmp2 = fArray[i].DeepClone();
                        fTmp2.Row = Row1 + j * diameter[0];
                        fTmp2.Column = col1;
                        fTmp2.Value = sarr[j];
                        fTmp2.ReviewValue = new JObject(new JProperty(fArray[i].SetKey, sarr[j]));
                        resultCelldata.Add(fTmp2);
                        // styleClone(sheetItem, false, fArray[i].Row, fArray[i].Column, Row1 + j * diameter[0], col1);
                    }
                    break;
                    #endregion
            }
            // 获取当前父格的子格
            var chilList = tmp.Where(x => x.LeftParentRow == fArray[i].Row && x.LeftParentColumn == fArray[i].Column
                                || x.TopParentRow == fArray[i].Row && x.TopParentColumn == fArray[i].Column);
            while (chilList.Any())
            {
                foreach (var chil in chilList)
                {
                    indexRecord[chil.Row + "-" + chil.Column] = 1;
                    // 获取子格的全部值
                    var (array2, pageCount2, total2) = getData(new DataCollectInput { SetCode = chil.SetCode});
                    
                    if (chil.LeftParentRow!=-1 && chil.TopParentRow != -1)
                    {   // 说明到了双父节点
                        // 1,获取双父的结果集中的值
                        // 1.2 上父结果集
                        var topList = resultCelldata.Where(x => x.CoordinateRow == chil.TopParentRow && x.CoordinateColumn == chil.TopParentColumn).ToList().BuildAdapter().AdaptToType<List<CellItem>>();
                        // 1.1 左父结果集
                        var leftList = resultCelldata.Where(x => x.CoordinateRow == chil.LeftParentRow && x.CoordinateColumn == chil.LeftParentColumn).ToList().BuildAdapter().AdaptToType<List<CellItem>>();
                        // 左父格，和上父格都加载过了才可以加载当前双父单元格
                        if (leftList.Any() && topList.Any())
                        {
                            // 记录已经填过的值
                            List<string> record = new List<string>();
                            // 先复刻原始值用于筛选
                            IEnumerable<DataRow> left = array2.Select().ToList();
                            IEnumerable<DataRow> last;
                            // List<CellItem> tmpList = new List<CellItem>();
                            HashSet<string> set = new HashSet<string>();
                            int lIndex = 0;
                            int tIndex = 0;
                            for (; lIndex < leftList.Count; lIndex++)
                            {
                                var lItemTmp = leftList[lIndex].DeepClone();
                                left = array2.Select().ToList();
                                // 1 先根据左父格确定子格的值
                                foreach (var property in lItemTmp.ReviewValue)
                                {
                                    if (array2.Columns.Contains(property.Key))
                                    {
                                        left = left.Where(x => x[property.Key].ToString() == property.Value.ToString());
                                    }
                                }
                                tIndex = 0;
                                for (; tIndex< topList.Count;tIndex++)
                                {
                                    var tItemTmp = topList[tIndex].DeepClone();
                                    last = left.ToList();
                                    // 2 再根据右父格继续确认子格的值
                                    foreach (var property in tItemTmp.ReviewValue)
                                    {
                                        if (array2.Columns.Contains(property.Key))
                                        {
                                            last = last.Where(x => x[property.Key].ToString() == property.Value.ToString());
                                        }
                                    }
                                    if (true)
                                    {
                                        // 3 先根据左父格定位坐标
                                        int[] differ = new int[] { chil.LeftParentRow - chil.Row, chil.LeftParentColumn - chil.Column };
                                        var chilTmp = chil.DeepClone();
                                        chilTmp.Row = lItemTmp.Row - differ[0];
                                        chilTmp.Column = lItemTmp.Column - differ[1];
                                        // 4 再根据上父格辅助定位
                                        if (lItemTmp.Expend != tItemTmp.Expend)
                                        {
                                            switch (tItemTmp.Expend)
                                            {
                                                case "cross":
                                                    chilTmp.Column = tItemTmp.Column - (chil.TopParentColumn - chil.Column);
                                                    break;
                                                case "portrait":
                                                    chilTmp.Row = tItemTmp.Row - (chil.TopParentRow - chil.Row);
                                                    break;
                                            }
                                        }
                                        // 5 填值，存入结果集中 （根据不同的数据设置来决定怎么渲染数据）
                                        // 5.1 即将合并单元格
                                        if(lItemTmp.ShowType == "group" && tItemTmp.ShowType == "group")
                                        {   // 必须左父格，上父格，和子格都设定为分组时才需要去重，否则全无
                                            if(chilTmp.ShowType == "group")
                                            {
                                                last = jTokenDistinct(array2,last, chil.SetKey);
                                            }else if (chil.ShowType == "summary")
                                            {
                                                var list = calcShowType(chil.ShowType, chil.ShowValue,last.Select(x => x[chil.SetKey]?.ToString() ?? "NA").ToList());
                                                DataRow tmpRes; 
                                                if (last.Count() == 0)
                                                {
                                                    tmpRes = left.First();
                                                    tmpRes[chil.SetKey] = 0;
                                                }
                                                else
                                                {
                                                    tmpRes = last.First();
                                                    tmpRes[chil.SetKey] = list[0];
                                                }
                                                List<DataRow> tmpList = new();
                                                tmpList.Add(tmpRes);
                                                last = tmpList;
                                            }
                                            switch (tItemTmp.Expend)
                                            {
                                                case "no":
                                                    #region 不拓展
                                                    StringBuilder sb = new StringBuilder();
                                                    foreach (var item in last)
                                                    {
                                                        sb.Append(item[chil.SetKey].ToString());
                                                        sb.Append(",");
                                                        record.Add(item.ToString());
                                                    }
                                                    sb = sb.Remove(sb.Length - 1, 1);
                                                    chilTmp.Value = sb.ToString();
                                                    // 先获取左父格的链路值
                                                    var reviewValue = lItemTmp.ReviewValue.ToObject<JObject>();
                                                    // 将右父格的链路值添加进入
                                                    foreach (var property in tItemTmp.ReviewValue)
                                                    {
                                                        if(!reviewValue.ContainsKey(property.Key))
                                                            reviewValue.Add(new JProperty(property.Key, property.Value));
                                                    }
                                                    if (!reviewValue.ContainsKey(chil.SetKey))
                                                        reviewValue.Add(new JProperty(chil.SetKey, chilTmp.Value));
                                                    chilTmp.ReviewValue = reviewValue;
                                                    resultCelldata.Add(chilTmp);
                                                    // styleClone(sheetItem, false, chil.Row, chil.Column, chilTmp.Row, chilTmp.Column);
                                                    break;
                                                #endregion
                                                case "cross":
                                                    #region 横向拓展
                                                    // 1 先推格子，大于当前加载列的数据都要往后推
                                                    if (last.Count() > 1)
                                                    {
                                                        int resultRecord = getRecord(indexRecord,"Column", chil.Column, chilTmp.Column);
                                                        if(last.Count() > resultRecord)
                                                        {
                                                            changeIndex(indexRecord,"Column",-1,chilTmp.Column,last.Count() - resultRecord);
                                                            changeRecord(indexRecord, "Column", -1, chilTmp.Column, last.Count() - resultRecord);
                                                            // 修改结果集的列信息
                                                            foreach (var item in resultCelldata.Where(x => x.Column > chilTmp.Column + resultRecord - 1))
                                                            {
                                                                item.Column += (last.Count()- resultRecord);
                                                            }
                                                            // 修改上父的集的列信息
                                                            foreach (var item in topList.Where(x => x.Column > chilTmp.Column + resultRecord - 1))
                                                            {
                                                                item.Column += (last.Count() - resultRecord);
                                                            }
                                                            // 修改左父的集的列信息
                                                            foreach (var item in leftList.Where(x => x.Column > chilTmp.Column + resultRecord - 1))
                                                            {
                                                                item.Column += (last.Count() - resultRecord);
                                                            }
                                                        }
                                                    }
                                                    // 2 开始添加数据
                                                    // 先获取左父格的链路值
                                                    var reviewValue2 = (JObject)lItemTmp.ReviewValue.DeepClone();
                                                    // 将右父格的链路值添加进入
                                                    foreach (var property in tItemTmp.ReviewValue)
                                                    {
                                                        if (!reviewValue2.ContainsKey(property.Key))
                                                            reviewValue2.Add(new JProperty(property.Key, property.Value));
                                                    }
                                                    if (!reviewValue2.ContainsKey(chil.SetKey))
                                                        reviewValue2.Add(new JProperty(chil.SetKey, chilTmp.Value));
                                                    chilTmp.ReviewValue = reviewValue2;
                                                    if (last.Count() == 0)
                                                    {
                                                        chilTmp.Value = "";
                                                        resultCelldata.Add(chilTmp);
                                                    }
                                                    foreach (var item in last)
                                                    {
                                                        record.Add(item.ToString());
                                                        chilTmp.Value = item[chil.SetKey].ToString();
                                                        resultCelldata.Add(chilTmp.DeepClone());
                                                        // styleClone(sheetItem, false, chil.Row, chil.Column, chilTmp.Row, chilTmp.Column);
                                                        chilTmp.Column += 1;
                                                    }
                                                    break;
                                                #endregion
                                                case "portrait":
                                                    #region 纵向拓展
                                                    // 1 先推格子，大于当前加载列的数据都要往后推
                                                    if (last.Count() > 1)
                                                    {
                                                        int resultRecord = getRecord(indexRecord, "Row", chil.Row, chilTmp.Row);
                                                        if (last.Count() > resultRecord)
                                                        {
                                                            changeIndex(indexRecord, "Row", -1, chilTmp.Row, last.Count() - resultRecord);
                                                            changeRecord(indexRecord, "Row", -1, chilTmp.Row, last.Count() - resultRecord);
                                                            // 修改结果集的列信息
                                                            foreach (var item in resultCelldata.Where(x => x.Row > chilTmp.Row + resultRecord - 1))
                                                            {
                                                                item.Row += (last.Count() - resultRecord);
                                                            }
                                                            // 修改上父的集的列信息
                                                            foreach (var item in topList.Where(x => x.Row > chilTmp.Row + resultRecord - 1))
                                                            {
                                                                item.Row += (last.Count() - resultRecord);
                                                            }
                                                            // 修改左父的集的列信息
                                                            foreach (var item in leftList.Where(x => x.Row > chilTmp.Row + resultRecord - 1))
                                                            {
                                                                item.Row += (last.Count() - resultRecord);
                                                            }
                                                        }
                                                    }
                                                    // 2 开始添加数据
                                                    // 先获取左父格的链路值
                                                    var reviewValue3 = lItemTmp.ReviewValue.ToObject<JObject>();
                                                    // 将右父格的链路值添加进入
                                                    foreach (var property in tItemTmp.ReviewValue)
                                                    {
                                                        if(!reviewValue3.ContainsKey(property.Key))
                                                            reviewValue3.Add(new JProperty(property.Key, property.Value));
                                                    }
                                                    if (!reviewValue3.ContainsKey(chil.SetKey))
                                                        reviewValue3.Add(new JProperty(chil.SetKey, chilTmp.Value));
                                                    chilTmp.ReviewValue = reviewValue3;
                                                    if(last.Count() == 0)
                                                    {
                                                        chilTmp.Value = "";
                                                        resultCelldata.Add(chilTmp);
                                                    }
                                                    foreach (var item in last)
                                                    {
                                                        record.Add(item.ToString());
                                                        chilTmp.Value = item[chil.SetKey].ToString();
                                                        resultCelldata.Add(chilTmp.DeepClone());
                                                        // styleClone(sheetItem, false, chil.Row, chil.Column, chilTmp.Row, chilTmp.Column);
                                                        chilTmp.Column += 1;
                                                    }
                                                    break;
                                                    #endregion
                                            }
                                            continue;
                                        }
                                        else
                                        {  // 此处代表父格中有一个数据设置为列表，说明子格的数据无论如何都只能列表显示
                                            if (set.Contains("left" + lIndex) || set.Contains("top" + tIndex))
                                            {
                                                chilTmp.ShowType = "list";
                                                chilTmp.Value = "";
                                                // 先获取左父格的链路值
                                                var reviewValue = (JObject)lItemTmp.ReviewValue.DeepClone();
                                                // 将右父格的链路值添加进入
                                                foreach (var property in tItemTmp.ReviewValue)
                                                {
                                                    if (!reviewValue.ContainsKey(property.Key))
                                                        reviewValue.Add(new JProperty(property.Key, property.Value));
                                                }
                                                if (!reviewValue.ContainsKey(chil.SetKey))
                                                    reviewValue.Add(new JProperty(chil.SetKey, chilTmp.Value));
                                                chilTmp.ReviewValue = reviewValue;
                                                resultCelldata.Add(chilTmp);
                                                // styleClone(sheetItem, false, chil.Row, chil.Column, chilTmp.Row, chilTmp.Column);
                                                continue;
                                            }
                                            foreach(var item in last)
                                            {
                                                if (record.Contains(item.ToString()))
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    chilTmp.ShowType = "list";
                                                    chilTmp.Value = item[chil.SetKey].ToString();
                                                    // 先获取左父格的链路值
                                                    var reviewValue = (JObject)lItemTmp.ReviewValue.DeepClone();
                                                    // 将右父格的链路值添加进入
                                                    foreach (var property in tItemTmp.ReviewValue)
                                                    {
                                                        if (!reviewValue.ContainsKey(property.Key))
                                                            reviewValue.Add(new JProperty(property.Key, property.Value));
                                                    }
                                                    if (!reviewValue.ContainsKey(chil.SetKey))
                                                        reviewValue.Add(new JProperty(chil.SetKey, chilTmp.Value));
                                                    chilTmp.ReviewValue = reviewValue;
                                                    record.Add(item.ToString());
                                                    resultCelldata.Add(chilTmp);
                                                    if(lItemTmp.ShowType == "list")
                                                    {
                                                        set.Add("left" + lIndex);
                                                    }
                                                    if (tItemTmp.ShowType == "list")
                                                    {
                                                        set.Add("top" + tIndex);
                                                    }
                                                    // styleClone(sheetItem, false, chil.Row, chil.Column, chilTmp.Row, chilTmp.Column);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // resultCelldata.AddRange(tmpList);
                        }
                        
                    }
                    else
                    {   // 子节点循环，以父为主，以子为辅
                        // 获取子节点的循环直径
                        // int[] chilDiameter = getCellCircInfo(tmp, chil);
                        // 获取结果集中所有父格加载出来的数据
                        List<CellItem> fcell ;
                        int[] differ; 
                        if(chil.LeftParentRow != -1)
                        {
                            differ = new int[] { chil.LeftParentRow-chil.Row, chil.LeftParentColumn - chil.Column };
                            fcell = resultCelldata.Where(x => x.CoordinateRow == chil.LeftParentRow && x.CoordinateColumn == chil.LeftParentColumn).ToList();
                        }
                        else
                        {
                            differ = new int[] { chil.TopParentRow - chil.Row, chil.TopParentColumn - chil.Column };
                            fcell = resultCelldata.Where(x => x.CoordinateRow == chil.TopParentRow && x.CoordinateColumn == chil.TopParentColumn).ToList();
                        }
                        // 这一步可有可无 排序
                        fcell = fcell.OrderBy(x => x.Row).ThenBy(x=>x.Column).ToList();
                        // 默认是分组
                        List<string> record = new List<string>();
                        // 遍历结果集中取到的父格
                        foreach (var cell in fcell)
                        {   // 获取父格的链路值 cell.ReviewValue
                            // 获取最终匹配的值
                            List<DataRow> last = array2.Select().ToList();
                            foreach ( var property in cell.ReviewValue)
                            {
                                if (array2.Columns.Contains(property.Key))
                                {
                                    last = last.Where(x => x[property.Key].ToString() == property.Value.ToString()).ToList();
                                }
                            }
                            // 根据父格子格之间的定位和子格的拓展方式来定位子格该显示的坐标
                            if (last.Any())
                            {
                                int Row = cell.Row; // differ[0];
                                int Column = cell.Column; //  differ[1];
                                // 1,获取子的初始定位 原则是根据和父格的定位差，来逐行逐列的计算定位差
                                #region 1.1 行定位差
                                int sign = 1;
                                int add = 0;
                                if (differ[0] != 0)
                                {
                                    if(differ[0] > 0)
                                    {
                                        sign = -1;
                                        add = -1;
                                    }
                                    for(int k = 0; k < Math.Abs(differ[0]); k++)
                                    {
                                        // Row += sign * getRecord(indexRecord, "Row", -1, Row+ (k > 0 ? 1 : 0) * sign+add);
                                        Row += sign * getRecord(indexRecord, "Row", -1, Row + add);
                                    }
                                    #region Row 再加上 拓展方向不等于最父格的单元格，circ
                                    List<CellItem> circList; 
                                    if (chil.Row > cell.CoordinateRow)
                                        circList = tmp.Where(x => x.Expend != "no" && x.Expend != fArray[i].Expend && x.Row >= cell.CoordinateRow && x.Row < chil.Row).ToList();
                                    else
                                        circList = tmp.Where(x => x.Expend != "no" && x.Expend != fArray[i].Expend && x.Row >= chil.Row && x.Row < cell.CoordinateRow).ToList();
                                    foreach (CellItem circ in circList)
                                    {
                                        foreach (var kv in indexRecord)
                                        {
                                            if (kv.Key.IndexOf("RowCircRecord" + circ.Row) != -1)
                                            {
                                                Row += kv.Value;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                                #region 1.2 列定位差
                                sign = 1;
                                add = 0;
                                if (differ[1] != 0)
                                {
                                    if (differ[1] > 0)
                                    {
                                        sign = -1;
                                        add = -1;
                                    }
                                    for (int k = 0; k < Math.Abs(differ[1]); k++)
                                    {
                                        // Column += sign * getRecord(indexRecord, "Column", -1, Column+(k>0?1:0) * sign + add);
                                        Column += sign * getRecord(indexRecord, "Column", -1, Column + add);
                                    }
                                    #region Row 再加上 拓展方向不等于最父格的单元格，circ (注释)
                                    // Row 再加上 拓展方向不等于最父格的单元格，circ
                                    List<CellItem> circList;
                                    if (chil.Column > cell.CoordinateColumn)
                                        circList = tmp.Where(x => x.Expend != "no" && x.Expend != fArray[i].Expend && x.Column >= cell.CoordinateColumn && x.Column < chil.Column).ToList();
                                    else
                                        circList = tmp.Where(x => x.Expend != "no" && x.Expend != fArray[i].Expend && x.Column >= chil.Column && x.Column < cell.CoordinateColumn).ToList();
                                    foreach (CellItem circ in circList)
                                    {
                                        foreach (var kv in indexRecord)
                                        {
                                            if (kv.Key.IndexOf("ColumnCircRecord" + circ.Column) != -1)
                                            {
                                                Column += kv.Value;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                                #region 以下，分组，列表，汇总的时候用
                                switch (cell.ShowType)
                                {
                                    case "group":
                                        #region 分组
                                        if (chil.ShowType == "group")
                                        {
                                            last = jTokenDistinct(array2,last, chil.SetKey).ToList();
                                        }else if(chil.ShowType == "summary")
                                        {
                                            var list = calcShowType(chil.ShowType, chil.ShowValue,last.Select(x => x[chil.SetKey]?.ToString()??"NA").ToList());
                                            var tmpRes = last.First();
                                            tmpRes[chil.SetKey] = list[0];
                                            last.Clear();
                                            last.Add(tmpRes);
                                        }
                                        switch (chil.Expend)
                                        {
                                            case "no":
                                                #region 不拓展
                                                StringBuilder sb = new StringBuilder();
                                                foreach (var item in last)
                                                {
                                                    sb.Append(item[chil.SetKey].ToString());
                                                }
                                                var chilTmp = chil.DeepClone();
                                                chilTmp.Row = Row;
                                                chilTmp.Column = Column;
                                                chilTmp.Value = sb.ToString();
                                                var reviewValue = cell.ReviewValue.ToObject<JObject>();
                                                if (!reviewValue.ContainsKey(chil.SetKey))
                                                    reviewValue.Add(new JProperty(chil.SetKey, chilTmp.Value));
                                                chilTmp.ReviewValue = reviewValue;
                                                resultCelldata.Add(chilTmp);
                                                // styleClone(sheetItem, false, chil.Row, chil.Column, Row, Column);
                                                #endregion
                                                break;
                                            case "cross":
                                                #region  横向拓展
                                                // 1,获取当前子格的循环直径
                                                int[] chilDiameter = getCellCircInfo(tmp, chil);
                                                #region 2 获取当前record
                                                bool sameFlag = cell.CoordinateColumn == chil.CoordinateColumn;
                                                int recordCount = getRecord(indexRecord, "Column", sameFlag ? fArray[i].Column: chil.CoordinateColumn, Column);
                                                // 获取拓展数
                                                int nowRecord = last.Count * Math.Abs(chil.ColumnMerge) * chilDiameter[1];
                                                // 获取父格拓展数
                                                int fRecord = getRecordByFcell("Column", fcell, cell);
                                                #endregion
                                                // 3,开始把结果集的数据往后推 ||(sameFlag && nowRecord > fRecord)
                                                if (nowRecord > recordCount || (fRecord>0 && fRecord < recordCount) )
                                                {
                                                    int expendNum;
                                                    if (fRecord <= 0)
                                                        expendNum = recordCount;
                                                    else
                                                        expendNum = Math.Min(recordCount, fRecord);
                                                    int num = nowRecord - expendNum;

                                                    getRecord(indexRecord, "Column", sameFlag ? fArray[i].Column : chil.CoordinateColumn, Column);
                                                    changeRecord(indexRecord, "Column", -1, Column, num);
                                                    changeIndex(indexRecord, "Column", -1, Column, num);
                                                    pushBack(resultCelldata, "Column", -1, Column + expendNum - 1, num);
                                                    /*foreach(CellItem item in fcell.Where(x=> x.Column > cell.Column))
                                                    {
                                                        item.Column += num;
                                                    }*/
                                                }
                                                // 4,开始填写数据

                                                for (int m =0;m<last.Count; m++)
                                                {
                                                    var chilTmp2 = chil.DeepClone();
                                                    chilTmp2.Row = Row;
                                                    chilTmp2.Column = Column + chilDiameter[1] * m;
                                                    chilTmp2.Value = last[m][chil.SetKey].ToString();
                                                    var reviewValue2 = cell.ReviewValue.ToObject<JObject>();
                                                    if (!reviewValue2.ContainsKey(chil.SetKey))
                                                        reviewValue2.Add(new JProperty(chil.SetKey, chilTmp2.Value));
                                                    chilTmp2.ReviewValue = reviewValue2;
                                                    resultCelldata.Add(chilTmp2);
                                                    // styleClone(sheetItem, false, chil.Row, chil.Column, Row, Column);
                                                }
                                                break;
                                            #endregion
                                            case "portrait":
                                                #region 纵向拓展
                                                // 1,获取当前子格的循环直径
                                                 int[] chilDiameter2 = getCellCircInfo( tmp, chil);
                                                #region 2 获取当前record
                                                bool sameFlag2 = cell.CoordinateRow == chil.CoordinateRow;
                                                int recordCount2 = getRecord(indexRecord, "Row", sameFlag2 ? cell.CoordinateRow : chil.CoordinateRow, Row);
                                                // int recordCount2 = getRecordByResultIndex(indexRecord, "Row",Row);
                                                // 获取当前拓展数
                                                int nowRecord2 = last.Count * Math.Abs(chil.RowMerge) * chilDiameter2[0];
                                                // 获取父格拓展数（当前父格，距离下一个父格的行差）
                                                int fRecord2 = getRecordByFcell("Row",fcell, cell);

                                                #endregion
                                                // 3,开始把结果集的数据往后推 || (sameFlag2 && nowRecord2 > fRecord2)
                                                if (nowRecord2 > recordCount2 || (fRecord2 > 0 && fRecord2 < recordCount2))
                                                {
                                                    int expendNum;
                                                    if (fRecord2 <= 0)
                                                        expendNum = recordCount2;
                                                    else
                                                        expendNum = Math.Min(recordCount2, fRecord2);
                                                    int num = nowRecord2 - expendNum;
                                                    // getRecord(indexRecord, "Row", sameFlag2 ? cell.Row : chil.CoordinateRow, Row);
                                                    changeRecord(indexRecord, "Row", -1, Row, num);
                                                    changeIndex(indexRecord, "Row", -1, Row, num);
                                                    pushBack(resultCelldata, "Row", -1, Row + expendNum - 1, num);
                                                }
                                                // 4,开始填写数据
                                                for (int m = 0; m < last.Count; m++)
                                                {
                                                    var chilTmp2 = chil.DeepClone();
                                                    chilTmp2.Row = Row + chilDiameter2[0] * m;
                                                    chilTmp2.Column = Column;
                                                    chilTmp2.Value = last[m][chil.SetKey].ToString();
                                                    var reviewValue2 = cell.ReviewValue.ToObject<JObject>();
                                                    if (!reviewValue2.ContainsKey(chil.SetKey))
                                                        reviewValue2.Add(new JProperty(chil.SetKey, chilTmp2.Value));
                                                    chilTmp2.ReviewValue = reviewValue2;
                                                    resultCelldata.Add(chilTmp2);
                                                    record.Add(last[m].ToString());
                                                    // styleClone(sheetItem, false, chil.Row, chil.Column, Row, Column);
                                                }
                                                break;
                                                #endregion
                                        }
                                        break;
                                    #endregion
                                    case "list":
                                        #region 父格列表，子格无论选择什么都是列表
                                        foreach (var item in last)
                                        {
                                            var index = array2.Rows.IndexOf(item);
                                            if (record.Contains(index.ToString()))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                
                                                List<string> arr = calcShowType(chil.ShowType, chil.ShowValue,null, item[chil.SetKey].ToString());
                                                
                                                var chilTmp = chil.DeepClone();
                                                chilTmp.ShowType = "list";
                                                chilTmp.Row = cell.Row - differ[0];
                                                chilTmp.Column = cell.Column - differ[1];
                                                chilTmp.Value = arr[0];
                                                var reviewValue = cell.ReviewValue.ToObject<JObject>();
                                                if (!reviewValue.ContainsKey(chil.SetKey))
                                                    reviewValue.Add(new JProperty(chil.SetKey, arr[0]));
                                                chilTmp.ReviewValue = reviewValue;
                                                record.Add(index.ToString());
                                                resultCelldata.Add(chilTmp);
                                                // styleClone(sheetItem, false, chil.Row, chil.Column, cell.Row - differ[0], cell.Column - differ[1]);
                                                break;
                                            }
                                        }
                                        break;
                                        #endregion
                                }
                                #endregion
                            }
                        }
                    }
                }
                List<CellItem> tmpChilList = new List<CellItem>();
                foreach (var chil in chilList)
                {
                    tmpChilList.AddRange(tmp.Where(x => x.LeftParentRow == chil.Row && x.LeftParentColumn == chil.Column
                                    || x.TopParentRow == chil.Row && x.TopParentColumn == chil.Column));
                }
                chilList = tmpChilList;
            }
        }
    }

    /// <summary>
    /// 加载块外行列内的数据
    /// </summary>
    private void outerCellLoading(List<CellItem> cellList , JArray blockList, Dictionary<string, int> indexRecord,
        List<CellItem> resultCelldata, CellItem cellItem)
    {
        int Row = cellItem.Row;
        int col = cellItem.Column;
        Boolean toBlock = false;
        string[] arr1 = new string[2];
        string[] arr2 = new string[2];
        switch (cellItem.SingleCellType)
        {
            case CellType.BLACK:
                // 空白不做操作
                break;
            case CellType.STATIC:
            case CellType.STATIC_MERGE:
                #region 静态元素无脑追随
                foreach (JObject block in blockList)
                {
                    arr1 = block["start"].ToString().Split(',');
                    arr2 = block["end"].ToString().Split(',');
                    // 1，获取块中所有单元格
                    var tmp = getBlockItems(cellList, arr1[0], arr1[1], arr2[0], arr2[1]);
                    // 2，获取最父格
                    var res = tmp.Where(x => x.LeftParentRow == -1 && x.TopParentRow == -1);
                    // 3，排序
                    List<CellItem> fArray = res.ToList().OrderBy(x => x.Row).ThenBy(x => x.Column).ToList();
                    // 确定“行”在块的范围内
                    if (Row >= Convert.ToInt32(arr1[0]) && Row <= Convert.ToInt32(arr2[0]))
                    {
                        // 此处，是否受影响跟子格有直接关系，跟父格有间接关系
                        #region 排序的第二种方法
                        /*List<CellItem> fArray =
                                            (from c in res
                                                orderby c.Row ascending, c.Column ascending
                                                select c).ToList();*/
                        #endregion
                        // 4，开始循环判断
                        foreach (var item in fArray)
                        {
                            if (item.Expend == "portrait" && item.Row == Row && col > item.Column)
                            {
                                toBlock = true;
                                break;
                            }
                            // 4.1 先判断左父格为当前单元格的子格
                            var chil = tmp.Where(
                                    x => x.LeftParentRow == item.Row &&
                                    x.LeftParentColumn == item.Column);
                            if (chil.Any())
                            {
                                // 判断比当前父类的行大
                                if (item.Row != Row)
                                {   // 因为在同一行寻找左父格为当前父类的子类

                                    // 寻找子类行大于等于cellItem 行的单元格，如果有，则循环遍历
                                    if (Row > item.Row)
                                        toBlock = chil.Where(x => x.Row > Row).Any();
                                    else
                                        toBlock = chil.Where(x => x.Row < Row).Any();
                                    if (toBlock) break;
                                }
                            }
                            // 4.2，判断上父格为当前单元格的子格
                            chil = tmp.Where(
                                    x => x.TopParentRow == item.Row &&
                                    x.TopParentColumn == item.Column);
                            if (chil.Any())
                            {
                                toBlock = chil.Where(x => x.Expend == "portrait" && x.Row == Row && col > x.Column).Any();
                                if (toBlock) break;
                            }
                        }
                    }
                    // 确定“列”在块的范围内
                    else if (col >= Convert.ToInt32(arr1[1]) && col <= Convert.ToInt32(arr2[1]))
                    {
                        // 此处，是否受影响跟子格有直接关系，跟父格有间接关系
                            
                        #region 排序的第二种方法
                        /*List<CellItem> fArray =
                                            (from c in res
                                                orderby c.Row ascending, c.Column ascending
                                                select c).ToList();*/
                        #endregion
                        // 4，开始循环判断
                        foreach (var item in fArray)
                        {
                            if (item.Expend == "cross" && item.Column == col && Row > item.Row)
                            {
                                toBlock = true;
                                break;
                            }
                            // 4.1 先判断上父格为当前单元格的子格
                            var chil = tmp.Where(
                                    x => x.TopParentRow == item.Row &&
                                    x.TopParentColumn == item.Column);
                            if (chil.Any())
                            {
                                // 判断比当前父类的行大
                                if (item.Column != col)
                                {   // 因为在同一行寻找左父格为当前父类的子类

                                    // 寻找子类行大于等于cellItem 行的单元格，如果有，则循环遍历
                                    if (col > item.Column)
                                        toBlock = chil.Where(x => x.Column > col).Any();
                                    else
                                        toBlock = chil.Where(x => x.Column < col).Any();
                                    if (toBlock) break;
                                }
                            }
                            // 4.2，判断左父格为当前单元格的子格
                            chil = tmp.Where(
                                    x => x.LeftParentRow == item.Row &&
                                    x.LeftParentColumn == item.Column);
                            if (chil.Any())
                            {
                                toBlock = chil.Where(x => x.Expend == "cross" && x.Column == col && Row > x.Row).Any();
                                if (toBlock) break;
                            }
                        }
                    }
                    if (toBlock)
                    {
                        inCellLoading(cellList,block, indexRecord, resultCelldata, cellItem);
                        // 此处加载块
                    }
                }
                    
                if(!toBlock)
                {   // 此处将当前单元格添加到返回值结果中
                    indexRecord[Row+"-"+col] = 1;
                    var (Row1, col1) = getIndex(indexRecord, Row, col);
                    var cell = cellItem.DeepClone();
                    cell.Row = Row1;
                    cell.Column = col1;
                    resultCelldata.Add(cell);
                    /*if (Row != Row1 || col != col1)
                    {
                        styleClone(sheetItem, true, Row, col, Row1, col1);
                    }
                    else { styleClone(sheetItem, false, Row, col, Row1, col1); }*/
                }
                break;
            #endregion
            case CellType.DYNAMIC:
            case CellType.DYNAMIC_MERGE:
                #region 动态元素，在块的行中，只有纵向才会跟随块，反之，只有横向
                foreach (JObject block in blockList)
                {
                    arr1 = block["start"].ToString().Split(',');
                    arr2 = block["end"].ToString().Split(',');
                    // 确定行在块的范围内
                    if (cellItem.Expend == "portrait" && Row >= Convert.ToInt32(arr1[0]) && Row <= Convert.ToInt32(arr2[0]))
                    {
                        // 1，获取块中所有单元格
                        var tmp = getBlockItems(cellList, arr1[0], arr1[1], arr2[0], arr2[1]);
                        // 2，获取最父格(且拓展方向为纵向)
                        var res = tmp.Where(x => x.LeftParentRow == -1 && x.TopParentRow == -1 && x.Expend == "portrait");
                        // 3，循环遍历父格
                        foreach (var item in res)
                        {
                            if (Row == item.Row)
                            {
                                toBlock = true;
                                break;
                            }
                            // 4.1 先判断左父格为当前单元格的子格
                            var chil = tmp.Where(
                                    x => x.LeftParentRow == item.Row &&
                                    x.LeftParentColumn == item.Column
                                    || x.TopParentRow == item.Row
                                    && x.TopParentColumn == item.Column);
                            if (chil.Any())
                            {
                                // 寻找 父子格行区间包含当前单元格子格
                                if (Row > item.Row)
                                    toBlock = chil.Where(x => x.Row > Row).Any();
                                else
                                    toBlock = chil.Where(x => x.Row < Row).Any();
                                if (toBlock) break;
                            }
                        }
                    }
                    // 确定列在块的范围内
                    else if (cellItem.Expend == "cross" && col >= Convert.ToInt32(arr1[1]) && col <= Convert.ToInt32(arr2[1]))
                    {
                        // 1，获取块中所有单元格
                        var tmp = getBlockItems(cellList, arr1[0], arr1[1], arr2[0], arr2[1]);
                        // 2，获取最父格(且拓展方向为横向)
                        var res = tmp.Where(x => x.LeftParentRow == -1 && x.TopParentRow == -1 && x.Expend == "cross");
                        // 3，循环遍历父格
                        foreach (var item in res)
                        {
                            if (col == item.Column)
                            {
                                toBlock = true;
                                break;
                            }
                            // 4.1 先判断左父格为当前单元格的子格
                            var chil = tmp.Where(
                                    x => x.LeftParentRow == item.Row &&
                                    x.LeftParentColumn == item.Column
                                    || x.TopParentRow == item.Row
                                    && x.TopParentColumn == item.Column);
                            if (chil.Any())
                            {
                                // 寻找 父子格行区间包含当前单元格子格
                                if (col > item.Column)
                                    toBlock = chil.Where(x => x.Column > col).Any();
                                else
                                    toBlock = chil.Where(x => x.Column < col).Any();
                                if (toBlock) break;
                            }
                        }
                    }
                    if (toBlock)
                    {
                        inCellLoading(cellList, block, indexRecord,resultCelldata, cellItem);// 此处加载块
                    }
                }
                if(!toBlock)
                {   // 正常填充
                    // 查询数据源
                    indexRecord[Row + "-" + col] = 1;
                    var (dt, pageCount, total) = getData(new DataCollectInput { SetCode = cellItem.SetCode});
                    
                    var arr = calcShowType( cellItem.ShowType, cellItem.ShowValue,
                            dt.Select().Select(x => x[cellItem.SetKey]?.ToString()).ToList());
                    switch (cellItem.Expend)
                    {
                        case "no":
                            #region 不拓展
                            StringBuilder sb = new StringBuilder();
                            foreach (var item in arr)
                            {
                                sb.Append(item);
                                sb.Append(",");
                            }
                            sb = sb.Remove(sb.Length - 1, 1);
                            var (Row1, col1) = getIndex(indexRecord, Row, col);
                            cellItem.Row = Row1;
                            cellItem.Column = col1;
                            cellItem.Value = sb.ToString();
                            resultCelldata.Add(cellItem);
                            /*if (Row != Row1 || col != col1)
                            {
                                styleClone(sheetItem, true, Row, col, Row1, col1);
                            }
                            else { styleClone(sheetItem, false, Row, col, Row1, col1); }*/
                            break;
                        #endregion
                        case "cross":
                            #region 横向拓展
                            int expendCount = indexRecord.GetOrAdd("ColumnRecord" + col,1);
                            // int changeCol = col;
                            var (Row2, col2) = getIndex(indexRecord, Row, col);
                            int num = (arr.Count() - expendCount) <= 0 ? 0 : (arr.Count() - expendCount);
                            if (num > 0)
                            { // 将大于等于当前列的拓展记录 + num
                                indexRecord["ColumnRecord" + col] = arr.Count();
                                changeIndex(indexRecord, "Column", col,-1, num);
                            }
                            for (int i = 0; i < arr.Count(); i++)
                            {
                                CellItem cell = cellItem.DeepClone();
                                cell.Row = Row2;
                                cell.Column = col2 + i;
                                cell.Value = arr[i];
                                resultCelldata.Add(cell);
                                // styleClone(sheetItem, false, Row, col, Row2, col2 + i);
                            }
                            break;
                        #endregion
                        case "portrait":
                            #region 纵向拓展
                            int expendCount2 = indexRecord.GetOrAdd("RowRecord" + Row,1);
                            // int changeRow = Row;
                            var (Row3, col3) = getIndex(indexRecord, Row, col);
                            int num2 = (arr.Count() - expendCount2) <= 0 ? 0 : (arr.Count() - expendCount2);
                            if (num2 > 0)
                            { // 将大于等于当前列的拓展记录 + num
                                indexRecord["RowRecord" + Row] = arr.Count();
                                changeIndex(indexRecord, "Row", Row,-1, num2);
                            }
                            for (int i = 0; i < arr.Count(); i++)
                            {
                                CellItem cell = cellItem.DeepClone();
                                cell.Row = Row3 + i;
                                cell.Column = col3;
                                cell.Value = arr[i];
                                resultCelldata.Add(cell);
                                // styleClone(sheetItem, false, Row, col, Row3 + i, col3);
                            }
                            break;
                            #endregion
                    }
                }
                break;
            #endregion
        }
    }
    /// <summary>
    /// 获取块中的所有单元格
    /// </summary>
    private List<CellItem> getBlockItems(List<CellItem> cellList, string v1, string v2, string v3, string v4)
    {
        return cellList.Where(x => x.Row >= Convert.ToInt32(v1)
                                        && x.Row <= Convert.ToInt32(v3)
                                        && x.Column >= Convert.ToInt32(v2)
                                        && x.Column <= Convert.ToInt32(v4)).ToList();
    }
    /// <summary>
    /// 获取sheet中的所有单元格
    /// </summary>
    private List<CellItem> getSheetItems(JObject sheetItem)
    {
        List<CellItem> list = new List<CellItem>();
        foreach (JObject item in sheetItem["celldata"])
        {
            list.Add(new CellItem(item));
        }
        return list;
    }

    /// <summary>
    /// 加载cell数据
    /// </summary>
    private void cellLoading(Dictionary<string, int> indexRecord,
        List<CellItem> resultCelldata, CellItem cellItem)
    {
        int Row = cellItem.Row;
        int col = cellItem.Column;
        var (dt, pageCount, total) = (new DataTable(),0L,0L);
        var (targetRow, targetCol) = (0,0);
        
        CellItem cellClone = cellItem.DeepClone();

        indexRecord[Row + "-" + col] = 1;

        switch (cellItem.SingleCellType)
        {
            case CellType.BLACK:
            case CellType.STATIC:
            case CellType.STATIC_MERGE:
                #region 静态单元格
                (targetRow, targetCol) = getIndex(indexRecord, Row, col);
                cellClone.Row = targetRow;
                cellClone.Column = targetCol;
                resultCelldata.Add(cellClone);
                break;
            #endregion
            case CellType.DYNAMIC:
            case CellType.DYNAMIC_MERGE:
                // 查询数据源
                (dt, pageCount, total) = getData(new DataCollectInput { SetCode = cellItem.SetCode});
                
                // 确定拓展方向
                string expend = cellItem.Expend;
                // jsonStr 值写入
                var arr = calcShowType(cellItem.ShowType, cellItem.ShowValue,
                    dt.Select().Select(x => x[cellItem.SetKey]?.ToString()).ToList());
                switch (expend)
                {
                    case "no":
                        #region 动态不拓展
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in arr)
                        {
                            sb.Append(item.ToString());
                        }
                        (targetRow, targetCol) = getIndex(indexRecord, Row, col);
                        cellClone.Row = targetRow;
                        cellClone.Column = targetCol;
                        cellClone.Value = sb.ToString();
                        resultCelldata.Add(cellClone);
                        break;
                    #endregion
                    case "cross":
                        #region 横向拓展
                        int expendCount = indexRecord.GetOrAdd("ColumnRecord" + col,1);
                        int ColumnMerge = cellClone.ColumnMerge == -1 ? 1 : cellClone.ColumnMerge;
                        (targetRow, targetCol) = getIndex(indexRecord, Row, col);
                        int num = (arr.Count() - expendCount) <= 0 ? 0 : (arr.Count() - expendCount);
                        if (num > 0)
                        { // 将大于等于当前列的拓展记录 + num
                            indexRecord["ColumnRecord" + col] = arr.Count();
                            changeIndex(indexRecord, "Column", col,-1, num);
                        }
                        for (int i = 0; i < arr.Count(); i++)
                        {
                            CellItem cellTmp = cellClone.DeepClone();
                            cellTmp.Row = targetRow;
                            cellTmp.Column = targetCol + i* ColumnMerge;
                            cellTmp.Value = arr[i];
                            resultCelldata.Add(cellTmp);
                            // styleClone(sheetItem, false, Row, col, Row3, col3 + i);
                        }
                                
                        break;
                    #endregion
                    case "portrait":
                        #region 纵向拓展
                        int expendCount2 = indexRecord.GetOrAdd("RowRecord" + Row,1);
                        int RowMerge = cellClone.RowMerge == -1 ? 1 : cellClone.RowMerge;
                        (targetRow, targetCol) = getIndex(indexRecord, Row, col);
                        int num2 = (arr.Count() - expendCount2) <= 0 ? 0 : (arr.Count() - expendCount2);
                        if (num2 > 0)
                        { // 将大于等于当前行的拓展记录 + num
                            indexRecord["RowRecord" + Row] = arr.Count();
                            changeIndex(indexRecord, "Row", Row,-1, num2);
                        }
                        for (int i = 0; i < arr.Count(); i++)
                        {
                            CellItem cellTmp = cellClone.DeepClone();
                            cellTmp.Row = targetRow + i* RowMerge;
                            cellTmp.Column = targetCol;
                            cellTmp.Value = arr[i];
                            resultCelldata.Add(cellTmp);
                            // styleClone(sheetItem, false, Row, col, Row4 + i, col4);
                        }
                        break;
                        #endregion
                }
                break;
        }
    }

    // ----------------------------------------------------------------------------------
    /// <summary>
    /// 获取解析器
    /// </summary>
    private V8ScriptEngine getResolver()
    {
        var engine = new V8ScriptEngine();
        engine.DocumentSettings.AccessFlags = Microsoft.ClearScript.DocumentAccessFlags.EnableFileLoading;
        engine.DefaultAccess = Microsoft.ClearScript.ScriptAccess.Full;
        return engine;
    }
    /// <summary>
    /// 执行js代码
    /// </summary>
    private bool getExpressionResult(string jsText, V8ScriptEngine engine)
    {
        StringBuilder sb = new StringBuilder("function say(){ return ");
        sb.Append(jsText);
        sb.Append("}");
        //V8Script script = engine.CompileDocument("./jsfiles/hellojs.js");   // 载入并编译js文件。
        engine.Execute(sb.ToString());  //直接执行js字符串
        var result = engine.Script.say();  //执行方法
        return result;
    }
    /// <summary>
    /// 将计算结果集按照需求进行计算
    /// </summary>
    private List<string> calcShowType(string showType, string showValue,List<string> sarr,string str = "")
    {
        if (!string.IsNullOrEmpty(str))
        {
            sarr = new List<string>();
            sarr.Add(str);
        }
        if(sarr == null || sarr.Count == 0)
        {
            sarr = new List<string>();
            sarr.Add("");
            return sarr;
        }
        switch (showType)
        {
            case "list":
                // 默认不操作，预留给以后做改动（万一呢）
                break;
            case "summary": // 汇总(此处因 在 loadingData 中做了计算，故此暂时放弃)
                // 求和 sum 平均 avg 最大值 max 最小值 min 个数 count
                if (BaseErrorCode.SummaryFlag)
                {
                    return sarr;
                }
                switch (showValue)
                {
                    case "sum":
                        // var (sum,numFlag) = sumSarr(sarr);
                        return new List<string>() { Enumerable.Sum(sarr.Select(x => double.Parse(x))).ToString() };
                    case "avg":
                        return new List<string>() { Enumerable.Average(sarr.Select(x => double.Parse(x))).ToString() };
                    case "max":
                        return new List<string>() { Enumerable.Max(sarr.Select(x => double.Parse(x))).ToString() };
                    case "min":
                        return new List<string>() { Enumerable.Min(sarr.Select(x => double.Parse(x))).ToString() };
                    case "count":
                        return new List<string>() { sarr.Count.ToString() };
                    case "countDistinct":
                        return new List<string>() { sarr.Where(x => x.IsNotNullOrEmpty()).Distinct().Count().ToString() };
                    default:
                        return new List<string>() { sarr.Count.ToString() };
                }
            case "group":
            default: 
                sarr = sarr.Distinct().ToList();
                break;
        }
        return sarr;
    }
    /// <summary>
    /// 集合字符串求和
    /// </summary>
    private (double,bool) sumSarr(List<string> sarr)
    {
        double sum = 0;
        bool numFlag = false;

        foreach(string s in sarr)
        {
            if (integerCheck(s))
            {
                numFlag = true;
                sum += Convert.ToDouble(s);
            }
        }
        return (sum,numFlag);
    }

    /// <summary>
    /// 获取块中单元格循环直径
    /// </summary>
    private int[] getCellCircInfo(List<CellItem> tmp, CellItem cellItem)
    {
        // 默认拓展信息
        int[] arr = new int[] { -1, -1, 0, 0 };
        int begin; int end;
        Boolean topf = false;
        // 取父格拓展信息
        #region 这里是取完整循环信息（注释）
        /*string key = null;
        if (cellItem.LeftParentRow != -1)
        {
            key = cellItem.LeftParentRow + "," + cellItem.LeftParentColumn;
        }
        else if (cellItem.TopParentRow != -1)
        {
            key = cellItem.TopParentRow + "," + cellItem.TopParentColumn;
        }
        if (key != null)
        {
            arr = diameterDic[key];
        }
        else
        {
            topf = true;
        }*/
        #endregion
        if (cellItem.LeftParentRow == -1 && cellItem.LeftParentColumn == -1)
        {
            topf = true;
        }

        switch (cellItem.Expend)
        {
            case "cross":
                if (arr[1] == -1)
                {
                    begin = cellItem.Column;
                    end = cellItem.Column;
                    // 横向拓展，应该计算父子关系的最大列差+1
                    IEnumerable<CellItem> chilList;
                    if (topf)
                    {
                        chilList = tmp.Where(x => x.LeftParentRow == -1 && x.TopParentRow == -1 && x.Expend == "cross");
                    }
                    else
                    {
                        chilList = tmp.Where(x => x.LeftParentRow == cellItem.Row && x.LeftParentColumn == cellItem.Column
                                        || x.TopParentRow == cellItem.Row && x.TopParentColumn == cellItem.Column);
                    }
                    while (chilList.Any())
                    {
                        foreach (CellItem item in chilList)
                        {
                            if (item.Column > end)
                            {
                                end = item.Column;
                            }
                            if (item.Column < begin)
                            {
                                begin = item.Column;
                            }
                        }
                        List<CellItem> res = new List<CellItem>();
                        foreach (CellItem item in chilList)
                        {
                            res.AddRange(tmp.Where(x => x.LeftParentRow == item.Row && x.LeftParentColumn == item.Column
                                    || x.TopParentRow == item.Row && x.TopParentColumn == item.Column));
                        }
                        chilList = res;
                    }
                    arr[1] = Math.Max(end - begin + 1, cellItem.ColumnMerge);
                    arr[3] = end - cellItem.Column+ Math.Max(cellItem.ColumnMerge,1) -1;
                }
                break;
            case "portrait":
                if (arr[0] == -1)
                {
                    begin = cellItem.Row;
                    end = cellItem.Row;
                    // 横向拓展，应该计算父子关系的最大列差+1
                    IEnumerable<CellItem> chilList2;
                    if (topf)
                    {
                        chilList2 = tmp.Where(x => x.LeftParentRow == -1 && x.TopParentRow == -1 && x.Expend == "portrait");
                    }
                    else
                    {
                        chilList2 = tmp.Where(x => x.LeftParentRow == cellItem.Row && x.LeftParentColumn == cellItem.Column
                                    || x.TopParentRow == cellItem.Row && x.TopParentColumn == cellItem.Column);
                    }
                    while (chilList2.Any())
                    {
                        foreach (CellItem item in chilList2)
                        {
                            if (item.Row > end)
                            {
                                end = item.Row;
                            }
                            if (item.Row < begin)
                            {
                                begin = item.Row;
                            }
                        }
                        List<CellItem> res = new List<CellItem>();
                        foreach (CellItem item in chilList2)
                        {
                            res.AddRange(tmp.Where(x => x.LeftParentRow == item.Row && x.LeftParentColumn == item.Column
                                    || x.TopParentRow == item.Row && x.TopParentColumn == item.Column));
                        }
                        chilList2 = res;
                    }
                    arr[0] = Math.Max(end - begin + 1, cellItem.RowMerge);
                    arr[2] = end - cellItem.Row + Math.Max(cellItem.RowMerge,1)-1;
                }
                break;
            default:
                break;
        }
        return arr;
    }

    /// <summary>
    /// List (Jtoken) 去重
    /// </summary>
    private IEnumerable<DataRow> jTokenDistinct(DataTable dt, IEnumerable<DataRow> last, string setDesc)
    {
        List<DataRow> dTmp = new();
        if (!last.Any() && dt.Columns.Contains(setDesc))
            return dTmp;
        List<string> d = new List<string>();    
        foreach (var item in last)
        {
            if (!d.Contains(item[setDesc].ToString()))
            {
                d.Add(item[setDesc].ToString());
                dTmp.Add(item);
            }
        }
        return dTmp;
    }
    /// <summary>
    /// 将 resultCellData 中的 targetCol 列后的数据向后推
    /// </summary>
    private void pushBack(List<CellItem> resultCelldata, string type, int setIndex,int nowIndex, int num)
    {
        switch (type)
        {
            case "Row":
                if(setIndex == -1)
                {
                    foreach (var item in resultCelldata.Where(x => x.Row > nowIndex))
                    {
                        item.Row += num;
                    }
                }
                else
                {
                    foreach (var item in resultCelldata.Where(x => x.CoordinateRow > setIndex))
                    {
                        item.Row += num;
                    }
                }
                break;
            case "Column":
                if (setIndex == -1)
                {
                    foreach (var item in resultCelldata.Where(x => x.Column > nowIndex))
                    {
                        item.Column += num;
                    }
                }
                else
                {
                    foreach (var item in resultCelldata.Where(x => x.CoordinateColumn > setIndex))
                    {
                        item.Column += num;
                    }
                }
                break;
        }
        
    }
    /// <summary>
    /// 根据指定结果集中的行，或者来获取，改行或者改列所有的拓展数量
    /// </summary>
    private int getRecordByResultIndex(Dictionary<string, int> indexRecord, string type, int index)
    {
        switch (type)
        {
            case "Row":
                foreach(var kv in indexRecord)
                {
                    if(kv.Value == index )
                    {
                        if(kv.Key.IndexOf("RowInit") != -1)
                        {
                            return getRecord(indexRecord, "Row", Convert.ToInt32(kv.Key.Replace("RowInit", "")), -1);
                        }
                        else if(kv.Key.IndexOf("resultRowInit"+ index) != -1)
                        {
                            // 返回当前单元格的全部拓展数量
                            int begin = indexRecord.GetOrAdd("resultRowInit" + index, 1);
                            int end = 0;
                            if (indexRecord.ContainsKey("resultRowRecord" + index))
                            {
                                string key = "";
                                foreach (var record in indexRecord)
                                {
                                    if (kv.Key.IndexOf("resultRowCircInit" + index) != -1)
                                    {
                                        if (indexRecord[record.Key] > end)
                                        {
                                            key = record.Key;
                                            end = indexRecord[record.Key];
                                        }
                                        // end = Math.Max(end, indexRecord[kv.Key]);
                                    }
                                }
                                if (end == 0)
                                {
                                    end = begin;
                                }
                                end += indexRecord.GetOrAdd(key.Replace("Init", "Record"), 1);
                                return end - begin;
                            }
                            else
                            {
                                return indexRecord.GetOrAdd("resultRowRecord" + index, 1);
                            }
                        }
                    }
                }
                break;
            case "Column":
                foreach (var kv in indexRecord)
                {
                    if (kv.Value == index)
                    {
                        if (kv.Key.IndexOf("ColumnInit") != -1)
                        {
                            return getRecord(indexRecord, "Column", Convert.ToInt32(kv.Key.Replace("ColumnInit", "")), -1);
                        }
                        else if (kv.Key.IndexOf("resultColumnInit" + index) != -1)
                        {
                            // 返回当前单元格的全部拓展数量
                            int begin = indexRecord.GetOrAdd("resultColumnInit" + index, 1);
                            int end = 0;
                            if (indexRecord.ContainsKey("resultColumnRecord" + index))
                            {
                                string key = "";
                                foreach (var record in indexRecord)
                                {
                                    if (kv.Key.IndexOf("resultColumnCircInit" + index) != -1)
                                    {
                                        if (indexRecord[record.Key] > end)
                                        {
                                            key = record.Key;
                                            end = indexRecord[record.Key];
                                        }
                                        // end = Math.Max(end, indexRecord[kv.Key]);
                                    }
                                }
                                if (end == 0)
                                {
                                    end = begin;
                                }
                                end += indexRecord.GetOrAdd(key.Replace("Init", "Record"), 1);
                                return end - begin;
                            }
                            else
                            {
                                return indexRecord.GetOrAdd("resultColumnRecord" + index, 1);
                            }
                        }
                    }
                }
                break;
        }
        return 0;
    }
    /// <summary>
    /// 获取指定行，或者和列的加载条数， 获取循环体的在指定行或者列的加载条数
    /// </summary>
    private int getRecord(Dictionary<string, int> indexRecord, string type, int index, int TargetIndex)
    {
        // 父子格定位的时候有使用
        if (index == -1)
        {
            foreach (var kv in indexRecord)
            {
                // && kv.Key.IndexOf("Circ") == -1
                if (kv.Key.IndexOf("Init") != -1  && kv.Key.IndexOf(type) != -1)
                {
                    if(kv.Value == TargetIndex)
                    {
                        return indexRecord.GetOrAdd(kv.Key.Replace("Init", "Record"), 1);
                    }else if (TargetIndex > kv.Value)
                    {
                        int tmp = indexRecord.GetOrAdd(kv.Key.Replace("Init", "Record"), 1);
                        if(kv.Value + tmp > TargetIndex)
                        {
                            return indexRecord.GetOrAdd(kv.Key.Replace("Init", "Record"), 1)-1;
                        }
                    } 
                }
            }
            return 1;
        }
        switch (type)
        {
            case "Row": // ColumnInit
                if (indexRecord.ContainsKey("RowInit" + index))
                {
                    if (TargetIndex != -1)
                    {
                        // 这里说明，有同一列的循环数据加载过了
                        // 先判断是不是首行
                        if(TargetIndex == indexRecord["RowInit" + index])
                        {
                            return indexRecord.GetOrAdd("RowRecord" + index, 1);
                        }
                        // 再次判断是不是改行已经有了记录
                        foreach (var kv in indexRecord)
                        {
                            if (kv.Key.IndexOf("Row") != -1 && kv.Key.IndexOf("Record") == -1 && kv.Value == TargetIndex) return indexRecord.GetOrAdd(kv.Key.Replace("Init", "Record"), 1);
                        }
                        // 最大循环数
                        int maxCirc = 1;
                        // 循环记录的关键字
                        string keyWord = "RowCircInit" + index;
                        // 当前循环数
                        int nowCirc = 1;
                        foreach (var kv in indexRecord)
                        {
                            if (kv.Key.IndexOf(keyWord) != -1)
                            {
                                nowCirc = Convert.ToInt32(kv.Key.Replace(keyWord+'-', ""));
                                maxCirc = Math.Max(nowCirc, maxCirc);
                                if (kv.Value == TargetIndex)
                                {
                                    return indexRecord.GetOrAdd("RowCircRecord" + index + "-" + nowCirc, 1);
                                }
                            }
                        }
                        // 此处说明没有当前循环的记录
                        // 1 添加记录
                        maxCirc++;
                        indexRecord[keyWord + "-" + maxCirc] = TargetIndex;
                        return indexRecord.GetOrAdd("RowCircRecord" + index + "-" + maxCirc, 1);
                    }
                    else
                    {   // 返回当前单元格的全部拓展数量
                        int begin = 0;
                        int end = 0;
                        if(indexRecord.ContainsKey("RowRecord" + index))
                        {
                            begin = indexRecord.GetOrAdd("RowInit" + index,1);
                            string key = "RowInit" + index;
                            foreach(var kv in indexRecord)
                            {
                                if(kv.Key.IndexOf("RowCircInit" + index) != -1)
                                {
                                    if(indexRecord[kv.Key] > end)
                                    {
                                        key = kv.Key;
                                        end = indexRecord[kv.Key];
                                    }
                                    // end = Math.Max(end, indexRecord[kv.Key]);
                                }
                            }
                            if(end == 0)
                            {
                                end = begin;
                            }
                            end += indexRecord.GetOrAdd(key.Replace("Init", "Record"),1);
                            return end - begin;
                        }
                        else
                        {
                            return indexRecord.GetOrAdd("RowRecord" + index, 1);
                        }
                    }
                }
                else
                {
                    // 此处说明没有当前列的记录信息，所以需要添加
                    getTargetRow(indexRecord, index);
                    // indexRecord["RowInit" + index] = TargetIndex==-1? index: TargetIndex;
                    return indexRecord.GetOrAdd("RowRecord" + index, 1);
                }
            case "Column": // ColumnInit
                if (indexRecord.ContainsKey("ColumnInit" + index))
                {
                    if (TargetIndex != -1)
                    {
                        // 这里说明，有同一列的循环数据加载过了
                        // 先判断是不是首列
                        if (TargetIndex == indexRecord["ColumnInit" + index])
                        {
                            return indexRecord.GetOrAdd("ColumnRecord" + index, 1);
                        }
                        // 再次判断是不是该列已经有了记录
                        foreach (var kv in indexRecord)
                        {
                            if (kv.Key.IndexOf("Column") != -1 && kv.Key.IndexOf("Record") == -1 &&  kv.Value == TargetIndex) return indexRecord.GetOrAdd(kv.Key.Replace("Init", "Record"), 1);
                        }
                        // 最大循环数
                        int maxCirc = 1;
                        // 循环记录的关键字
                        string keyWord = "ColumnCircInit" + index;
                        // 当前循环数
                        int nowCirc = 1;
                        foreach (var kv in indexRecord)
                        {
                            if (kv.Key.IndexOf(keyWord) != -1)
                            {
                                nowCirc = Convert.ToInt32(kv.Key.Replace(keyWord + '-', ""));
                                maxCirc = Math.Max(nowCirc, maxCirc);
                                if (kv.Value == TargetIndex)
                                {
                                    return indexRecord.GetOrAdd("ColumnCircRecord" + index + "-" + nowCirc, 1);
                                }
                            }
                        }
                        // 此处说明没有当前循环的记录
                        // 1 添加记录
                        maxCirc++;
                        indexRecord[keyWord + "-" + maxCirc] = TargetIndex;
                        return indexRecord.GetOrAdd("ColumnCircRecord" + index + "-" + maxCirc, 1);
                    }
                    else
                    {   // 返回当前列的全部拓展数量
                        int begin = 0;
                        int end = 0;
                        if (indexRecord.ContainsKey("ColumnRecord" + index))
                        {
                            begin = indexRecord.GetOrAdd("ColumnInit" + index,1);
                            string key = "ColumnInit" + index;
                            foreach (var kv in indexRecord)
                            {
                                if (kv.Key.IndexOf("ColumnCircInit" + index) != -1)
                                {
                                    if (indexRecord[kv.Key] > end)
                                    {
                                        key = kv.Key;
                                        end = indexRecord[kv.Key];
                                    }
                                    // end = Math.Max(end, indexRecord[kv.Key]);
                                }
                            }
                            if (end == 0)
                            {
                                end = begin;
                            }
                            end += indexRecord.GetOrAdd(key.Replace("Init", "Record"), 1);
                            return end - begin;
                        }
                        else
                        {
                            return indexRecord.GetOrAdd("ColumnRecord" + index, 1);
                        }
                    }
                }
                else
                {
                    // 此处说明没有当前列的记录信息，所以需要添加
                    getTargetColumn(indexRecord, index);
                    // indexRecord["ColumnInit" + index] = TargetIndex == -1 ? index : TargetIndex;
                    return indexRecord.GetOrAdd("ColumnRecord" + index, 1);
                }
        }
        return -1;
    }

    /// <summary>
    /// 获取已加载的父格拓展数量
    /// </summary>
    private int getRecordByFcell(string type, IEnumerable<CellItem> fcell, CellItem cell)
    {
        int min;
        IEnumerable<CellItem> list;
        switch (type)
        {
            case "Row":
                list = fcell.Where(x => x.Row > cell.Row);
                if (!list.Any())
                    return 0;
                min = list.Select(x => x.Row).Min();
                return min - cell.Row;
            case "Column":
                list = fcell.Where(x => x.Column > cell.Column);
                if (!list.Any())
                    return 0;
                min = list.Select(x => x.Column).Min();
                return min - cell.Column;
        }
        return 0;
    }
    /// <summary>
    /// 修改指定行，或者和列的加载条数， 修改循环体的在指定行或者列的加载条数
    /// </summary>
    private void changeRecord(Dictionary<string, int> indexRecord, string type, int setIndex, int nowIndex, int num)
    {
        switch (type)
        {
            case "Column":
                if(setIndex != -1)
                {
                    indexRecord["ColumnRecord" + setIndex] = indexRecord.GetOrAdd("ColumnRecord" + setIndex,1) + num;
                }
                else if(nowIndex != -1)
                {
                    foreach(KeyValuePair<string, int> kv in indexRecord)
                    {
                        if (kv.Key.IndexOf("Column") != -1 && kv.Key.IndexOf("Init") != -1 && kv.Value == nowIndex){
                            string keyTmp = kv.Key.Replace("Init", "Record");
                            if (indexRecord.ContainsKey(keyTmp)){
                                indexRecord[keyTmp] = indexRecord[keyTmp] + num;
                            }
                            else
                            {
                                indexRecord[keyTmp] = num + 1;
                            }
                        }
                    }
                }
                break;
            case "Row":
                if (setIndex != -1)
                {
                    indexRecord["RowRecord" + setIndex] = indexRecord.GetOrAdd("RowRecord" + setIndex,1) + num;
                }
                else if (nowIndex != -1)
                {
                    foreach (KeyValuePair<string, int> kv in indexRecord)
                    {
                        // kv.Key.IndexOf("RowCircInit") != -1 &&
                        if (kv.Key.IndexOf("Row") !=-1 && kv.Key.IndexOf("Init") != -1 && kv.Value == nowIndex)
                        {
                            string keyTmp = kv.Key.Replace("Init", "Record");
                            if (indexRecord.ContainsKey(keyTmp))
                            {
                                indexRecord[keyTmp] = indexRecord[keyTmp] + num;
                            }
                            else
                            {
                                indexRecord[keyTmp] = num + 1;
                            }
                        }
                    }
                }
                break;
        }
    }
    /// <summary>
    /// 将大于等于当前列的拓展记录 + num
    /// </summary>
    private void changeIndex(Dictionary<string, int> indexRecord, string type, int setIndex,int nowIndex, int num)
    {
        switch (type)
        {
            case "Column":
                if(setIndex != -1)
                {
                    foreach (KeyValuePair<string, int> kv in indexRecord)
                    {
                        if (kv.Key.Contains("ColumnInit") && Convert.ToInt32(kv.Key.Replace("ColumnInit", "")) > setIndex)
                        {
                            indexRecord[kv.Key] += num;
                        }
                        if (kv.Key.Contains("ColumnCircInit") && Convert.ToInt32(kv.Key.Replace("ColumnCircInit","").Split('-')[0]) > setIndex)
                        {
                            indexRecord[kv.Key] += num;
                        }
                    }
                }
                else if(nowIndex != -1)
                {
                    foreach (KeyValuePair<string, int> kv in indexRecord)
                    {
                        if(kv.Key.Contains("ColumnInit") || kv.Key.Contains("ColumnCircInit") )
                        {
                            if(kv.Value > nowIndex)
                            {
                                indexRecord[kv.Key] += num;
                            }
                        }
                    }
                }
                break;
            case "Row":
                if (setIndex != -1)
                {
                    foreach (KeyValuePair<string, int> kv in indexRecord)
                    {
                        if (kv.Key.Contains("RowInit") && Convert.ToInt32(kv.Key.Replace("RowInit", "")) > setIndex)
                        {
                            indexRecord[kv.Key] += num;
                        }
                        if (kv.Key.Contains("RowCircInit") && Convert.ToInt32(kv.Key.Replace("RowCircInit", "").Split('-')[0]) > setIndex)
                        {
                            indexRecord[kv.Key] += num;
                        }
                    }
                }
                else if (nowIndex != -1)
                {
                    foreach (KeyValuePair<string, int> kv in indexRecord)
                    {
                        if (kv.Key.Contains("RowInit") || kv.Key.Contains("RowCircInit"))
                        {
                            if (kv.Value > nowIndex)
                            {
                                indexRecord[kv.Key] += num;
                            }
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 获取父格的定位信息
    /// </summary>
    private (int, int) getFatherIndex(Dictionary<string, int> indexRecord, string type, int Row, int col)
    {
        bool RowFlag = false;
        //bool colFlag = false;
        // 先判断是不是已经加载过了行和列
        int reRow = col;
        if (indexRecord.ContainsKey("RowInit" + col))
        {
            reRow = indexRecord["RowInit" + col];
            RowFlag = true;
        }
        int reCol = col;
        if (indexRecord.ContainsKey("ColumnInit" + col))
        {
            reCol = indexRecord["ColumnInit" + col];
            //colFlag = true;
        }

        // 先找出是不是在循环辐射范围内
        foreach (KeyValuePair<string, int> kv in indexRecord)
        {
            if(!RowFlag && kv.Key.IndexOf("RowRelation")!= -1 
                && Convert.ToInt32(kv.Key.Replace("RowRelation","")) < Row
                && kv.Value > Row)
            {// 此时代表该行未加载过，但是在循环行范围内

            }
        }
        return (reRow,reCol);
    }
    /// <summary>
    /// 根据当前遍历的坐标，获取指定的最新坐标
    /// </summary>
    private (int Row, int col) getIndex(Dictionary<string, int> indexRecord, int Row, int col)
    {
        int reRow = getTargetRow(indexRecord, Row);
        int reCol = getTargetColumn(indexRecord, col);
        return (reRow, reCol);
    }
    /// <summary>
    ///  根据设定列，获取改列的初始化列
    /// </summary>
    private int getTargetColumn(Dictionary<string, int> indexRecord, int col)
    {
        int reCol = col;
        if (indexRecord.ContainsKey("ColumnInit" + col))
        {
            reCol = indexRecord["ColumnInit" + col];
        }
        else
        {
            foreach (var item in indexRecord)
            {
                if (item.Key.IndexOf("ColumnRecord") > -1
                && Convert.ToInt32(item.Key.Replace("ColumnRecord", "")) < col)
                {
                    reCol += item.Value - 1;
                }
                if (item.Key.IndexOf("ColumnCircRecord") > -1)
                {
                    if (Convert.ToInt32(item.Key.Replace("ColumnCircRecord", "").Split("-")[0]) < col)
                    {
                        reCol += item.Value - 1;
                    }
                }
            }
            indexRecord["ColumnInit" + col] = reCol;
            indexRecord["ColumnRecord" + col] = 1;
        }
        return reCol;
    }
    /// <summary>
    /// 根据设定行，获取改行的初始化行
    /// </summary>
    private int getTargetRow(Dictionary<string, int> indexRecord, int Row)
    {
        int reRow = Row;
        if (indexRecord.ContainsKey("RowInit" + Row))
        {
            reRow = indexRecord["RowInit" + Row];
        }
        else
        {
            foreach (var item in indexRecord)
            {
                if (item.Key.IndexOf("RowRecord") > -1
                && Convert.ToInt32(item.Key.Replace("RowRecord", "")) < Row)
                {
                    reRow += item.Value - 1;
                }
                if (item.Key.IndexOf("RowCircRecord") > -1)
                {
                    if (Convert.ToInt32(item.Key.Replace("RowCircRecord", "").Split("-")[0]) < Row)
                    {
                        reRow += item.Value-1;
                    }
                }
            }
            indexRecord["RowInit" + Row] = reRow;
            indexRecord["RowRecord" + Row] = 1;
        }
        return reRow;
    }

    /// <summary>
    /// 查询数据集的值
    /// </summary>
    (DataTable, long, long) getData(DataCollectInput dataSet)
    {
        if (keyValuePairs.ContainsKey(dataSet.SetCode))
            return keyValuePairs[dataSet.SetCode];
        else
        {
            DateTime dt = DateTime.Now;
            var res = dataCollectServices.testTransform(dataSet).Result;
            keyValuePairs.TryAdd(dataSet.SetCode,(res.Item4,res.Item2,res.Item3));
            res = (null,0,0,null);
            GC.Collect();
            DateTime dt1 = DateTime.Now;
            double t = dt1.Subtract(dt).TotalMilliseconds;
            return keyValuePairs[dataSet.SetCode];
        }
    }

    /// <summary>
    /// 移除数据集的值
    /// </summary>
    void removeData(DataCollectInput dataSet)
    {
        var (dt, num1, num2) = (new DataTable(),0L,0L);
        var result = (dt, num1, num2);
        if (keyValuePairs.ContainsKey(dataSet.SetCode))
             keyValuePairs.TryRemove(dataSet.SetCode, out result);
    }

    /// <summary>
    /// 单元格样式复刻
    /// </summary>
    private void styleClone(JObject sheetItem, Boolean deleteFlag, int cellR, int cellC, int Row, int Column)
    {
        // 边框
        JObject config = sheetItem["config"].ToObject<JObject>();
        if (config.ContainsKey("borderInfo") && config["borderInfo"].Count() > 0)
        {
            int i = 0;
            foreach (JObject item in config["borderInfo"])
            {
                if (item.ContainsKey("range"))
                {
                    if (cellR >= (int)item.SelectToken("range[0].Row[0]")
                        && cellR <= (int)item.SelectToken("range[0].Row[1]")
                        && cellC >= (int)item.SelectToken("range[0].Column[0]")
                        && cellC <= (int)item.SelectToken("range[0].Column[1]"))
                    {
                        // 满足条件，添加边框
                        JObject addItem = (JObject)item.DeepClone();
                        addItem["range"][0]["Row"][0] = Row;
                        addItem["range"][0]["Row"][1] = Row;
                        addItem["range"][0]["Column"][0] = Column;
                        addItem["range"][0]["Column"][1] = Column;
                        JArray jarray = sheetItem["config"]["borderInfo"].ToObject<JArray>();
                        jarray.Add(addItem);
                        if (deleteFlag && cellR != Row && cellC != Column)
                        {
                            jarray[i].Remove();
                        }
                        sheetItem["config"]["borderInfo"] = jarray;
                        break;
                    }
                }
                i++;
            }
        }
        // 行宽
        if (config.ContainsKey("Rowlen"))
        {
            JObject Rowlen = config["Rowlen"].ToObject<JObject>();
            if (Rowlen.ContainsKey(cellR.ToString()))
            {
                Rowlen[Row.ToString()] = Rowlen[cellR.ToString()];
                sheetItem["config"]["Rowlen"] = Rowlen;
            }
        }
        // 列宽
        if (config.ContainsKey("Columnlen"))
        {
            JObject Columnlen = config["Columnlen"].ToObject<JObject>();
            if (Columnlen.ContainsKey(cellC.ToString()))
            {
                Columnlen[Column.ToString()] = Columnlen[cellC.ToString()];
                sheetItem["config"]["Columnlen"] = Columnlen;
            }
        }
    }

    /// <summary>
    /// 整理数据集的请求参数对象 DataCollectInput
    /// </summary>
    DataCollectInput getDataSet(string setCode, string setParam, int requestCount, int pageSize)
    {
        DataCollectInput dto = new DataCollectInput();
        dto.SetCode = setCode;
        if (requestCount <= 0) requestCount = 1;
        if (pageSize <= 0) pageSize = 1;
        dto.LimitStart = (requestCount - 1) * pageSize + 1;
        dto.LimitEnd = (requestCount) * pageSize;
        dto.DataSetParamDtoList = getContextData(setParam, setCode);
        return dto;
    }
    /// <summary>
    /// 整理参数列表
    /// </summary>
    private List<DataCollectItem> getContextData(String setParam,string setCode)
    {
        List<DataCollectItem> list = new List<DataCollectItem>();
        if (!string.IsNullOrEmpty(setParam))
        {
            JObject setParamJson = JObject.Parse(setParam);
            // 查询条件
            if (setParamJson.ContainsKey(setCode))
            {
                JToken jToken = setParamJson[setCode];
                foreach (JProperty property in jToken)
                {
                    DataCollectItem item = new DataCollectItem();
                    item.ParamName = property.Name;
                    item.SampleItem = jToken[property.Name].ToString();
                    list.Add(item);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 标记所有的最父格
    /// </summary>
    private void checkFinalFather(List<CellItem> cellList)
    {
        var fTmp = cellList.Where(x => x.LeftParentRow == -1 && x.TopParentRow == -1);
        foreach (CellItem cellItem in fTmp)
        {
            if (cellList.Where(x => x.LeftParentRow == cellItem.Row && x.LeftParentColumn == cellItem.Column
                                || x.TopParentRow == cellItem.Row && x.TopParentColumn == cellItem.Column).Any())
            {
                cellItem.FinalFather = true;
            }
        }
    }
    /// <summary>
    /// 判断是否为数字
    /// </summary>
    private Boolean integerCheck(string Value)
    {
        return Regex.IsMatch(Value, @"^(-?\d+)(\.\d+)?$");
    }
    /// <summary>
    /// 判断是否只包含字母和数字并且同时包含
    /// </summary>
    private static Boolean functionCheck(string Value)
    {
        if (Regex.IsMatch(Value, @"^[a-zA-Z0-9]*$"))
            return Regex.IsMatch(Value, @"^(?![^\d]+$)(?![^a-zA-Z]+$)[^\u4e00-\u9fa5\s]+$");
        else
            return false;
    }

}
