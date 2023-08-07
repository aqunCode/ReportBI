using Bi.Core.Interfaces;
using Bi.Entities.Entity;
using SqlSugar;

namespace Bi.Services.IService;

/// <summary>
/// 针对不用数据库不用语法选择器
/// </summary>
public interface IDbEngineServices : IDependency
{
    /// <summary>
    /// 根据数据源类型获取所有的用户名用于前台选择的下拉框
    /// </summary>
    /// <param name="sourceType">数据源类型</param>
    /// <returns></returns>
    string showUsers(string sourceType);
    /// <summary>
    /// 返回sql（查询当前连接串能访问的所有表）
    /// </summary>
    /// <param name="sourceType">数据源类型</param>
    /// <param name="user">用户/database/schema</param>
    /// <returns></returns>
    string showTables(string sourceType, string user);
    /// <summary>
    /// 返回sql（查询当前表所有列）
    /// </summary>
    /// <param name="sourceType">数据源类型</param>
    /// <param name="tableName">表名</param>
    /// <returns></returns>
    string showColumns(string sourceType, string tableName,string user = "");
    /// <summary>
    /// 根据数据源类型，链接串链接数据库
    /// </summary>
    /// <param name="sourceType">数据源类型</param>
    /// <param name="sourceConnect">数据源链接串</param>
    /// <returns></returns>
    (SqlSugarScope, String) GetRepository(string sourceType, String sourceConnect);
    /// <summary>
    /// 分页查询sql组装
    /// </summary>
    /// <param name="sql">要进行分页的sql</param>
    /// <param name="limitStart">从第几行开始</param>
    /// <param name="limitEnd">到第几行结束</param>
    /// <param name="sourceType">数据源类型</param>
    /// <returns></returns>
    string sqlPageRework(string sql, int limitStart, int limitEnd, string sourceType);
    /// <summary>
    /// 根据函数名称，获取指定数据库的函数表达形式
    /// </summary>
    /// <param name="functionName">函数名称</param>
    /// <param name="sourceType">数据源类型</param>
    /// <returns></returns>
    string showFunction(string functionName, string sourceType);
    /// <summary>
    /// 根据函数名称，获取指定数据库的函数名称
    /// </summary>
    /// <param name="functionName">函数名称</param>
    /// <param name="sourceType">数据源类型</param>
    /// <returns></returns>
    string showFunctionName(string functionName, string sourceType);
    /// <summary>
    /// 判断当前函数是否是聚合函数
    /// </summary>
    /// <param name="calculatorFunction"></param>
    /// <returns></returns>
    bool checkAggregate(string calculatorFunction);
    /// <summary>
    /// 根据指定数组，生成不同数据库的order by 函数
    /// </summary>
    /// <param name="sortBy">排序类型</param>
    /// <param name="fieldName">排序字段名称</param>
    /// <param name="dataType">字段类型</param>
    /// <param name="sortValue">数组，sortBy的值为manual手动</param>
    /// <param name="sourceType">数据源类型</param>
    /// <returns></returns>
    string showOrderBy(string sortBy, string fieldName, string dataType, List<SortField> sortValue, string sourceType);
    /// <summary>
    /// 生成不同数据库的查指定字符的默认sql
    /// </summary>
    /// <param name="field">要执行查询的数字和字符 eg：1 23 'aaa' 's' '个'</param>
    /// <param name="sourceType">数据源类型</param>
    /// <returns></returns>
    string showDefaultSql(string field, string sourceType);
    /// <summary>
    /// 根据SQL获取对应的TableName
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    string getTablesName(string sql);
}

