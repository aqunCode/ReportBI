using Bi.Core.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;

namespace Bi.Core.SqlSugar;

public static class SqlSugarSetup
{
    public static void AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        List<ConnectionConfig> listConfig = new List<ConnectionConfig>();

        List<SqlSugarOptins> DbList = configuration.GetSection("SqlSugarDB").Get<List<SqlSugarOptins>>();

        services.AddSingleton<ISqlSugarClient>(o =>
        {
            DbList.ForEach(x =>
            {
                listConfig.Add(new ConnectionConfig()
                {
                    ConfigId = x.ConnId,
                    ConnectionString = x.ConnString,
                    DbType = x.DbType,
                    IsAutoCloseConnection = true,

                    //AopEvents = new AopEvents 
                    //{
                    //    //SQL执行前
                    //    OnLogExecuting = (sql, parm) =>
                    //    {

                    //    },
                    //    //SQL执行完
                    //    OnLogExecuted = (sql, parm) =>
                    //    {

                    //    },
                    //    //SQL出错
                    //    OnError = (ex) =>
                    //    {
                    //        logger.LogError($"错误SQL：{ex.Sql},参数：{ex.Parametres}");
                    //    }
                    //}
                });
            });
            //return new SqlSugarScope(listConfig);
            var db = new SqlSugarScope(listConfig);

            listConfig.ForEach(x =>
            {
                string ConfigId = x.ConfigId;
                //SQL执行前
                db.GetConnectionScope(ConfigId).Aop.OnLogExecuting = (sql, parm) =>
                {
                    if (environment?.IsDevelopment() == true)
                    {
                        Console.ResetColor();
                        Console.WriteLine($"【数据库】：{ConfigId},【SQL语句】：{sql},{GetParams(parm)}");
                    }
                };
                //SQL执行完
                db.GetConnectionScope(ConfigId).Aop.OnLogExecuted = (sql, parm) =>
                {
                    if (db.Ado.SqlExecutionTime.TotalMilliseconds > 600000)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        LogHelper.Info($"超时10Min,【操作时间】：{db.Ado.SqlExecutionTime.TotalMilliseconds},【数据库】：{ConfigId},【SQL语句】：{sql},{GetParams(parm)}");
                        Console.ResetColor();
                    }
                };
                //SQL出错
                db.GetConnectionScope(ConfigId).Aop.OnError = (ex) =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    LogHelper.Error($"SQL报错,【数据库】：{ConfigId},【SQL语句】：{ex.Sql},【SQL参数】：{ex.Parametres}");
                    Console.ResetColor();
                };
            });

            return db;
        });
    }

    private static string GetParams(SugarParameter[] pars)
    {
        string key = "【SQL参数】：";
        foreach (var param in pars)
        {
            key += $"{param.ParameterName}:{param.Value}\n";
        }

        return key;
    }
}

