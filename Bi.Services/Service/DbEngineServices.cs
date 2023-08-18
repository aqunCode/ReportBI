using Bi.Core.Extensions;
using Bi.Entities.Entity;
using Bi.Services.IService;
using SqlSugar;
using System.Text;
using System.Text.RegularExpressions;

namespace Bi.Services.Service;

internal class DbEngineServices : IDbEngineServices
{
    /// <summary>
    /// 根据数据源类型获取所有的用户名用于前台选择的下拉框
    /// </summary>
    public string showUsers(string sourceType)
    {
        string sql;
        switch (sourceType)
        {
            case "Sqlite":
                sql = $@"show databases;";
                break;
            case "Hive":
                sql = $@"show databases;";
                break;
            case "Spark":
                sql = $@"show databases";
                break;
            case "MySql":
                sql = $@"show databases;";
                break;
            case "ClickHouse":
                sql = $@"show databases;";
                break;
            case "PostgreSql":
                sql = $@"SELECT schema_name FROM information_schema.schemata";
                break;
            case "SqlServer":
                sql = $@"select name from sys.sysdatabases;";
                break;
            case "Oracle":
                sql = $@"SELECT USERNAME FROM DBA_USERS 
                        WHERE ACCOUNT_STATUS = 'OPEN'
                        AND DEFAULT_TABLESPACE <> 'SYSTEM'
                        ORDER BY USERNAME ASC";
                break;
            default: //默认情况下认为是Mysql
                sql = $@"show databases;";
                break;
        }
        return sql;
    }
    /// <summary>
    /// 返回sql（查询当前连接串能访问的所有表）
    /// </summary>
    public string showTables(string sourceType, string user)
    {
        string sql;
        switch (sourceType)
        {
            case "Sqlite":
                sql = @"select name from sqlite_master where type='table' order by name";
                break;
            case "Hive":
                sql = @"show tables";
                break;
            case "Spark":
                user = user.IsNullOrEmpty() ? "default" : user;
                sql = $@"show tables from {user};";
                break;
            case "MySql":
                user = user.IsNullOrEmpty() ? "mysql" : user;
                sql = $@"select table_schema,table_name 
                        from information_schema.tables
                        where TABLE_SCHEMA = '{user}'";
                break;
            case "ClickHouse":
                user = user.IsNullOrEmpty() ? "system" : user;
                sql = $@" select distinct name from system.tables where database = '{user}'
                         order by name asc";
                break;
            case "PostgreSql":
                user = user.IsNullOrEmpty() ? "rptprod" : user;
                sql = $@"select schemaname,tablename tablename from pg_tables 
                        where schemaname = '{user}'
                        order by tablename asc";
                break;
            case "SqlServer":
                sql = $@"select '{user}.dbo' owner ,name table_name from {user}..sysobjects where xtype='U'";
                break;
            case "Oracle":
                user = user.IsNullOrEmpty() ? "LEDRPT" : user;
                sql = $@"SELECT OWNER,table_name FROM all_tables
                        WHERE duration IS NULL 
                        AND OWNER = '{user}'
                        ORDER BY table_name ASC";
                break;
            default: //默认情况下认为是Mysql
                user = user.IsNullOrEmpty() ? "mysql" : user;
                sql = $@"select table_schema OWNER,table_name 
                        from information_schema.tables
                        where TABLE_SCHEMA = '{user}'";
                break;
        }
        return sql;
    }
    /// <summary>
    /// 返回sql（查询当前表所有列）
    /// </summary>
    public string showColumns(string sourceType, string tableName,string user = "")
    {
        string sql = "";
        switch (sourceType)
        {
            case "Sqlite":// sqllite 不支持查询表结构
                sql = $@" PRAGMA  table_info({tableName})";
                break;
            case "Hive":
                sql = $@" desc {tableName}";
                break;
            case "Spark":
                sql = $@" SHOW COLUMNS in  {tableName}";
                if (user.IsNotNullOrEmpty())
                    sql = string.Concat(new string[]
                    {
                        sql,
                        " in ",
                        user
                    });
                break;
            case "MySql":
                sql = $@"SELECT   TABLE_NAME tableName 						-- 表名
                               , COLUMN_NAME columnName						-- 列名
                               , case when COLUMN_COMMENT='' then COLUMN_NAME else COLUMN_COMMENT end  columnComment				-- 列注释
                               , DATA_TYPE columnType						-- 字段类型 varchar之类的
                        FROM INFORMATION_SCHEMA.COLUMNS
                        where TABLE_NAME = '{tableName}'";
                break;
            case "ClickHouse":
                sql = @" select   '暂不支持' tableName
		                        , '暂不支持' columnName
		                        , '暂不支持' columnComment
		                        , '暂不支持' columnType";
                break;
            case "PostgreSql":
                sql = $@"select    a.relname tableName
		                        , b.attname columnName
		                        , COALESCE(c.description ,b.attname) columnComment
		                        , d.typname columnType
                        from pg_class a
                        left join pg_attribute b on a.oid = b.attrelid 
                        left join pg_description c on b.attrelid  = c.objoid and b.attnum = c.objsubid 
                        left join pg_type d on b.atttypid = d.oid
                        where a.relname = '{tableName}'
                        and b.attname not in ('gp_segment_id','tableoid','ctid','xmax','xmin','cmax','cmin')";
                break;
            case "SqlServer":
                sql = $@"select b.name tablename,a.name columnname,a.name columnComment,c.name columnType
　　                   from {user}..syscolumns a 
　　                   INNER JOIN {user}..sysobjects b on  a.id=b.id 
　　                    left join {user}..systypes c on a.xtype = c.xtype
　　                    where B.name='{tableName}'";
                break;
            case "Oracle":
                sql = $@"SELECT 
	                          b.TABLE_NAME tableName					-- 表名
	                        , c.COLUMN_NAME columnName					-- 列名
	                        , nvl(f.COMMENTS,c.COLUMN_NAME) columnComment					-- 列注释
	                        , c.DATA_TYPE columnType					-- 字段类型 varchar之类的
                        FROM  all_tables b 
                        LEFT JOIN all_tab_columns c ON b.TABLE_NAME = c.TABLE_NAME 
                        LEFT JOIN all_col_comments f ON  b.TABLE_NAME = f.TABLE_NAME AND c.COLUMN_NAME = f.COLUMN_NAME 
                        WHERE b.TABLE_NAME IN ('{tableName}')";
                if (user.IsNotNullOrEmpty())
                    sql = string.Concat(sql,$" AND b.OWNER = '{user}' ");
                break;
            default: //默认情况下认为是Mysql
                sql = $@"SELECT   TABLE_NAME tableName 						-- 表名
                               , COLUMN_NAME columnName						-- 列名
                               , case when COLUMN_COMMENT='' then COLUMN_NAME else COLUMN_COMMENT end  columnComment				-- 列注释
                               , DATA_TYPE columnType						-- 字段类型 varchar之类的
                        FROM INFORMATION_SCHEMA.COLUMNS
                        where  TABLE_NAME = '{tableName}'";
                break;
        }
        return sql;
    }
    /// <summary>
    /// 根据数据源类型，链接串链接数据库
    /// </summary>
    public (SqlSugarScope, String) GetRepository(string sourceType, String sourceConnect)
    {
        switch (sourceType)
        {
            case "PostgreSql":
                sourceType = "PostgreSQL";
                break;
            case "Hive":
                sourceType = "Odbc";
                break;
            case "Spark":
                sourceType = "Custom";
                break;
            default:
                break;
        }
        SqlSugar.DbType type = (SqlSugar.DbType)Enum.Parse(typeof(SqlSugar.DbType), sourceType== "Doris" ? "MySql" : sourceType);
        String checkSql;
        switch (type)
        {
            case SqlSugar.DbType.Oracle:
                checkSql = " select 1 from dual ";
                break;
            case SqlSugar.DbType.MySql:
                checkSql = " select 1 ";
                break;
            case SqlSugar.DbType.SqlServer:
                checkSql = " select 1 value";
                break;
            default:
                checkSql = " select 1 ";
                break;
        }
        if (!string.IsNullOrEmpty(sourceConnect))
        {
            SqlSugarScope sqlSugar = new SqlSugarScope(
                     new ConnectionConfig
                     {
                         ConnectionString = sourceConnect,
                         IsAutoCloseConnection = true,
                         DbType = type
                     }
                );

            #region Doris 查询之前需要加上此句
            if (sourceType == "Doris")
                sqlSugar.Ado.ExecuteCommand("SWITCH hudi;use `default`; show tables;");
            #endregion

            return (sqlSugar, checkSql);
        }
        else
        {
            return (null, null);
        }

    }
    /// <summary>
    /// 分页查询sql组装
    /// </summary>
    public string sqlPageRework(string sql, int limitStart, int limitEnd, string sourceType)
    {
        StringBuilder sb = new();

        switch (sourceType)
        {
            case "Sqlite":
            case "Hive":
            case "MySql":
            case "Doris":
            case "ClickHouse":
                sb.Append(" select * from(");
                sb.Append(sql);
                sb.Append(") t");
                sb.Append(" limit ");
                sb.Append(limitStart);
                sb.Append(" , ");
                sb.Append(limitEnd - limitStart);
                break;
            case "PostgreSql":
                sb.Append(" select * from(");
                sb.Append(sql);
                sb.Append(") t");
                sb.Append(" limit ");
                sb.Append(limitEnd - limitStart);
                sb.Append(" offset ");
                sb.Append(limitStart);
                break;
            case "Spark":   //  spark 不支持当前写法，以下为自定义写法，后面会处理
                sb.Append(" select * from(");
                sb.Append(sql);
                sb.Append(") t");
                sb.Append(" limit ");
                sb.Append(limitEnd);
                sb.Append(" offset ");
                sb.Append(limitStart);
                break;
            case "SqlServer":
                sb.Append(" select top ");
                sb.Append(limitEnd);
                sb.Append(" * ");
                sb.Append(" from(");
                sb.Append(sql);
                sb.Append(") t");
                break;
            case "Oracle":
            default: //默认情况下认为是Oracle
                sb.Append(" select * from(");
                sb.Append(sql);
                sb.Append(") t");
                sb.Append(" OFFSET ");
                sb.Append(limitStart);
                sb.Append("  ROWS FETCH NEXT  ");
                sb.Append(limitEnd - limitStart);
                sb.Append(" ROWS ONLY");
                break;
        }
        return sb.ToString();
    }
    /// <summary>
    /// 根据函数名称，获取指定数据库的函数表达形式
    /// </summary>
    /// <returns></returns>
    public string showFunction(string functionName, string sourceType)
    {
        string function = "";
        switch (sourceType)
        {
            case "Sqlite":
                switch (functionName)
                {
                    default:
                        function = " {columnName} ";
                        break;
                }
                break;
            case "Hive":
                switch (functionName)
                {
                    default:
                        function = " {columnName} ";
                        break;
                }
                break;
            case "MySql":
                switch (functionName)
                {
                    default:
                        function = " {columnName} ";
                        break;
                }
                break;
            case "ClickHouse":
                switch (functionName)
                {
                    default:
                        function = " {columnName} ";
                        break;
                }
                break;
            case "PostgreSql":
                switch (functionName)
                {
                    default:
                        function = " {columnName} ";
                        break;
                }
                break;
            case "SqlServer":
                switch (functionName)
                {
                    default:
                        function = " {columnName} ";
                        break;
                }
                break;
            case "Spark":
                switch (functionName)
                {
                    case "count":
                        function = " COUNT({columnName}) ";
                        break;
                    case "countDistinct":
                        function = " COUNT( DISTINCT {columnName}) ";
                        break;
                    case "toChar":
                        function = " {columnName} ";
                        break;
                    case "toNumber":
                        function = " TO_NUMBER({columnName}) ";
                        break;
                    case "max":
                        function = " MAX({columnName}) ";
                        break;
                    case "min":
                        function = " MIN({columnName}) ";
                        break;
                    case "sum":
                        function = " SUM({columnName}) ";
                        break;
                    case "avg":
                        function = " ROUND(AVG({columnName}),5) ";
                        break;
                    case "stdev":
                        function = " ROUND(STDDEV({columnName}),5) ";
                        break;
                    case "YYYY":   //年
                        function = " TO_CHAR({columnName},'YYYY') ";
                        break;
                    case "MM":     //月
                        function = " TO_CHAR({columnName},'YYYY-MM') ";
                        break;
                    case "DD":    //日
                        function = " TO_CHAR({columnName},'YYYY-MM-dd') ";
                        break;
                    case "HH":    //日时 24小时制
                        function = " TO_CHAR({columnName},'YYYY-MM-dd HH24') ";
                        break;
                    case "Q":     //季
                        function = " TO_CHAR({columnName},'Q') ";
                        break;
                    case "WK":    //周
                        function = " TO_CHAR({columnName},'IW') ";
                        break;
                    default:
                        function = " {columnName} ";
                        break;
                }
                break;
            case "Oracle":
                switch (functionName)
                {
                    case "count":
                        function = " COUNT({columnName}) ";
                        break;
                    case "countDistinct":
                        function = " COUNT( DISTINCT {columnName}) ";
                        break;
                    case "toChar":
                        function = " TO_CHAR({columnName}) ";
                        break;
                    case "toNumber":
                        function = " TO_NUMBER({columnName}) ";
                        break;
                    case "max":
                        function = " MAX({columnName}) ";
                        break;
                    case "min":
                        function = " MIN({columnName}) ";
                        break;
                    case "sum":
                        function = " SUM({columnName}) ";
                        break;
                    case "avg":
                        function = " ROUND(AVG({columnName}),5) ";
                        break;
                    case "stdev":
                        function = " ROUND(STDDEV({columnName}),5) ";
                        break;
                    case "YYYY":   //年
                        function = " TO_CHAR({columnName},'YYYY') ";
                        break;
                    case "MM":     //月
                        function = " TO_CHAR({columnName},'YYYY-MM') ";
                        break;
                    case "DD":    //日
                        function = " TO_CHAR({columnName},'YYYY-MM-dd') ";
                        break;
                    case "HH":    //日时 24小时制
                        function = " TO_CHAR({columnName},'YYYY-MM-dd HH24') ";
                        break;
                    case "Q":     //季
                        function = " TO_CHAR({columnName},'Q') ";
                        break;
                    case "WK":    //周
                        function = " TO_CHAR({columnName},'IW') ";
                        break;
                    default:
                        function = " {columnName} ";
                        break;
                }
                break;
            default: //默认情况下认为是Oracle
                break;
        }
        return function;
    }
    /// <summary>
    /// 根据函数名称，获取指定数据库的函数名称
    /// </summary>
    /// <returns></returns>
    public string showFunctionName(string functionName, string sourceType) 
    {
        string function = "";
        switch (sourceType)
        {
            case "Sqlite":
                break;
            case "Hive":
                break;
            case "MySql":
                break;
            case "ClickHouse":
                break;
            case "PostgreSql":
                break;
            case "SqlServer":
                break;
            case "Spark":
                switch (functionName)
                {
                    case "COUNTDISTINCT":
                        function = " COUNT( DISTINCT ";
                        break;
                    case "COUNT":
                        function = " COUNT";
                        break;
                    case "CHAR":
                        function = " TO_CHAR";
                        break;
                    case "INT":
                        function = " TO_NUMBER";
                        break;
                    case "DATE":
                        function = " TO_DATE";
                        break;
                    case "ABS":
                        function = " ABS";
                        break;
                    case "IF":
                        function = "  CASE WHEN ";
                        break;
                    case "ELSEIF":
                        function = " WHEN ";
                        break;
                    default:
                        function = functionName;
                        break;
                }
                break;
            case "Oracle":
                switch (functionName)
                {
                    case "COUNTDISTINCT":
                        function = " COUNT( DISTINCT ";
                        break;
                    case "COUNT":
                        function = " COUNT";
                        break;
                    case "CHAR":
                        function = " TO_CHAR";
                        break;
                    case "INT":
                        function = " TO_NUMBER";
                        break;
                    case "DATE":
                        function = " TO_DATE";
                        break;
                    case "ABS":
                        function = " ABS";
                        break;
                    case "IF":
                        function = "  CASE WHEN ";
                        break;
                    case "ELSEIF":
                        function = " WHEN ";
                        break;
                    default:
                        function = functionName;
                        break;
                }
                break;
            default: //默认情况下认为是Oracle
                function = functionName;
                break;
        }
        return function;
    }
    /// <summary>
    /// 根据指定数组，生成不同数据库的order by 函数
    /// </summary>
    /// <returns></returns>
    public string showOrderBy(string sortBy, string fieldName, string dataType, List<SortField> sortValue, string sourceType)
    {
        StringBuilder function = new();
        switch (sourceType)
        {
            case "Sqlite":
                break;
            case "Hive":
                break;
            case "MySql":
                break;
            case "ClickHouse":
                break;
            case "PostgreSql":
                break;
            case "SqlServer":
                break;
            case "Oracle":
                switch (sortBy)
                {
                    case "asc":
                        function.Append(fieldName);
                        function.Append(" ASC ") ;
                        break;
                    case "desc":
                        function.Append(fieldName);
                        function.Append(" DESC ");
                        break;
                    case "manual":// 代表是手动排序  ORDER BY DECODE(DATASETCODE,'5',0,'3',1,'1',2) 
                        if(sortValue == null)
                        {
                            function.Append(fieldName);
                            function.Append(" ASC ");
                            break;
                        }
                        function.Append(" DECODE(");
                        function.Append(fieldName);
                        function.Append(',');
                        int i = 0;
                        foreach (var item in sortValue)
                        {
                            if(dataType == "Number")
                            {
                                function.Append((string)item.Value);
                            }
                            else
                            {
                                function.Append('\'');
                                function.Append((string)item.Value);
                                function.Append('\'');
                            }
                            function.Append(',');
                            function.Append(i);
                            function.Append(',');
                            i++;
                        }
                        function = function.Remove(function.Length-1,1);
                        function.Append(')');
                        break;
                    default:
                        function.Append(fieldName);
                        //function.Append(" ASC "); order by 默认升序
                        break;
                }
                break;
            default: //默认情况下认为是Oracle
                break;
        }
        return function.ToString();
    }
    /// <summary>
    /// 判断是否为聚合函数（为后续筛查groupBy字段）
    /// </summary>
    /// <param name="calculatorFunction"></param>
    /// <returns></returns>
    public bool checkAggregate(string calculatorFunction)
    {
        string[] arr = { "COUNT(", "COUNTDISTINCT(", "MAX(", "MIN(", "SUM(", "AVG(", "STDDEV(" };
        foreach(var item in arr)
        {
            if (calculatorFunction.Contains(item))
                return true;
        }
        return false;
    }

    public string showDefaultSql(string field, string sourceType)
    {
        StringBuilder function = new();
        switch (sourceType)
        {
            case "MySql":
            case "Sqlite":
            case "Hive":
            case "ClickHouse":
            case "PostgreSql":
            case "SqlServer":
                function.Append("SELECT ");
                function.Append(field);
                break;
            case "Oracle":
                function.Append("SELECT ");
                function.Append(field);
                function.Append(" FROM DUAL ");
                break;
            default:
                function.Append("SELECT ");
                function.Append(field);
                break;
        }
        return function.ToString();
    }

    /// <summary>
    /// 获取SQL语句,获取对应相关的TableName
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public string getTablesName(string sql)
    {
        var sqlUpper = sql.ToUpper();

        if (!sqlUpper.Contains("JOIN") && sqlUpper.Contains("WHERE"))
        {
            var start = sqlUpper.IndexOf("FROM");
            var end = sqlUpper.IndexOf("WHERE");
            //去除用户，别名
            // var table = sqlUpper.Substring(start + 4, end - start - 4).TrimStart().Split('.')[1].Split(' ')[0];
            var table = sqlUpper.Substring(start + 4, end - start - 4).TrimStart();
            //var tableSummary = table.Split(",").SelectMany(t => $"'{t.Split(" ")[0]}',");  //说明有多表关联  
            var tableSummary = table.Split(",").Select(t => t.Split(" ")[0]);  //说明有多表关联    
            var tableResult = string.Empty;
            foreach (var item in tableSummary)
            {
                //tableResult += "'";
                tableResult += item.Contains(".") ? item.Split('.')[1] + "'," : item + "',";
                //tableResult += "',";
            }

            tableResult.Replace(",", ",'").TrimEnd(",'".ToArray());
        }

        //List<string> tables = new List<string>();

        var tables = new StringBuilder();
        Regex regex = new Regex(@"((?![^(]*\))(?![^']*')(?i)(?:FROM|JOIN)\s+([\w\.]+))", RegexOptions.Multiline);
        MatchCollection matches = regex.Matches(sql);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            tables.Append($"{match.Groups[2].Value.Split('.')[1]},");
        }

        return tables.ToString().Replace(",", "','").TrimEnd(",'".ToArray());

    }

}
