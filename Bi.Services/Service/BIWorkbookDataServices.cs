using Bi.Core.Extensions;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using SqlSugar;
using System.Data;

namespace Bi.Services.Service;

public class BIWorkbookDataServices: IBIWorkbookDataServices
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;

    private IDbEngineServices dbEngine;

    private DataSetServices dataSetService;

    public BIWorkbookDataServices(  ISqlSugarClient _sqlSugarClient,
                                    IDbEngineServices dbService,
                                    DataSetServices dataSetService)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("BaiZeRpt");
        this.dbEngine = dbService;
        this.dataSetService = dataSetService;
    }


    /// <summary>
    /// 获取当前DB用户下的表的所有字段
    /// </summary>
    public async Task<(string, IEnumerable<ColumnInfo>)> getTableColumn(BIWorkbookInput inputs)
    {
        #region 展示所有表字段     
        //1.获取除自定义字段以外的所有表字段
        var columninfos = await getColumninfo(inputs.DatasetId);
        List<ColumnInfo> columns  = (List<ColumnInfo>)columninfos.Item2;

        //2.加入BI_CUSTOMER_FIELD 自定义字段
        var customColumn = (await repository.Queryable<BiCustomerField>().Where(x => x.DatasetId == inputs.DatasetId && x.DeleteFlag == "N").ToListAsync());
        
        for (int i = 0; i < customColumn.Count; i++)
        {
            //customColumn[i]是每一个循环得到的对象，BiCustomerField
            //BiCustomerField singlerow = customColumn[i];
            //创建一个columninfo对象

            // 维度转换的删除原始字段
            if(customColumn[i].Remark == "1")
            {
                var tmp = columns.Where(x => x.ColumnName == customColumn[i].FieldCode && x.LabelName == customColumn[i].LabelName).FirstOrDefault();
                if (tmp != null)
                {
                    columns.Remove(tmp);
                }
            }

            ColumnInfo customcolnuminfo = new ColumnInfo
            {
                NodeId = customColumn[i].Id,
                LabelName = customColumn[i].LabelName,
                ColumnName = customColumn[i].FieldCode,
                DataType = customColumn[i].DataType,
                ColumnType = customColumn[i].Remark
            };
            //将customColumn[i]这个对象的需要的值赋给columninfo对象
            //colnuminfo.tableName = customColumn[i].TableName;
            //colnuminfo.ColumnName = customColumn[i].FieldCode;
            ////BiCustomerField字段类型
            //colnuminfo.dataType=(customColumn[i].Remark=="0" || customColumn[i].Remark == "2") ? "Number": "String";            
            //colnuminfo.columnType = customColumn[i].Remark;

            //将columninfo对象添加到columns list集合里
            columns.AddRange(customcolnuminfo);
        }
        
        #endregion

        #region  模糊查询字段或表名
        if (!string.IsNullOrEmpty(inputs.ColumnName))
        {
            List<ColumnInfo> selectcolumns = new List<ColumnInfo>();
            for (int i = 0; i < columns.Count; i++)
            { 
                if (!string.IsNullOrEmpty(columns[i].ColumnName) && columns[i].ColumnName.ToUpper().Contains(inputs.ColumnName.ToUpper()) || columns[i].LabelName.ToUpper().Contains(inputs.ColumnName.ToUpper()))
                {
                    selectcolumns.AddRange(columns[i]);
                }
            }
            return ("OK", selectcolumns);
        }
        
        
        #endregion

        return ("OK", columns);

    }


    public async Task<(string,IEnumerable<ColumnInfo>)> getColumninfo(string datasetId)
    {
        //1.根据数据集id查询BI_DATASET_NODE
        var dataSets = await repository.Queryable<BiDatasetNode>().Where(x => x.DatasetCode == datasetId && x.DeleteFlag == "N" && x.Enabled == 1).ToListAsync();

        //2.根据SOURCECODE查询auto_data_source获取SOURCETYPE
        string sourceCode = dataSets[0].SourceCode;
        var dataSource = (await repository.Queryable<DataSource>().Where(x => x.SourceCode == sourceCode && x.DeleteFlag == 0 && x.Enabled == 1).ToListAsync()).FirstOrDefault();
        if (dataSource == null)
            return ("数据源不存在或者禁用", null);
        var engine = dbEngine.GetRepository(dataSource.SourceType, dataSource.SourceConnect);

        List<ColumnInfo> columns = new List<ColumnInfo>();
        for (int i = 0; i < dataSets.Count; i++)
        {
            string temp = dataSets[i].TableName;
            var arr = temp.Split('.');

            var sql = dbEngine.showColumns(dataSource.SourceType, arr[1], arr[0]);
            //var columnInfos = await engine.Item1.SqlQueryable<ColumnInfo>(sql).ToListAsync();
            var res = await dataSetService.getColumnlist(new TableInput
            {
                SourceCode = sourceCode,
                Type = dataSource.SourceType,
                TableName = temp
            });
            if (res.Item1 != "OK")
                return ($"tablename[{temp}] 获取表结构失败", null);
            //ColumnInfo columnfullinfos = new ColumnInfo();    //new 在循环里每次创建新的对象，否则永远是同一个对象不会被改变
            foreach (var item in res.Item2)
            {
                ColumnInfo columnfullinfo = new ColumnInfo();
                columnfullinfo.NodeId = dataSets[i].Id;
                columnfullinfo.LabelName = dataSets[i].NodeLabel;     //NodeLabel取代tableName
                columnfullinfo.ColumnName = item.ColumnName;
                columnfullinfo.ColumnType = item.ColumnType;
                columnfullinfo.ColumnComment = item.ColumnComment;
                //根据数据字典GITEA.SYS_DATAITEM_DETAIL 字段类型区分维度、指标、时间
                string datatypesql = $@"SELECT sdd.DETAILNAME datatype FROM GITEA.SYS_DATAITEM_DETAIL sdd WHERE sdd.ITEMID='CB3DB2280BB1478F86A625D5D3F0CD03' AND sdd.DETAILCODE LIKE '%{item.ColumnType}%'";
                var datatype = await repository.Ado.GetDataTableAsync(datatypesql);
                //var datatype = await repository.Queryable<String>(datatypesql).ToListAsync();
                if (datatype != null)
                {
                    columnfullinfo.DataType = datatype.Rows[0][0].ToString();
                }
                else
                    columnfullinfo.DataType = "String";
                //return ("请维护数据字典",null);
                columns.AddRange(columnfullinfo);
            }
        }
        return ("OK", columns);
    }

    
}
