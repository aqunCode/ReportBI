using Bi.Core.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.SqlSugar
{
    public static class SqlSugarSetup
    {
        static Process process;
        static CancellationTokenSource tokenSource;

        public static void AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            // 此处添加功能启动spark驱动
            var urls = configuration["Urls"];
            //careSparkServerAsync(urls);

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
                        if(environment?.IsDevelopment() == true)
                        {
                            Console.ResetColor();
                            Console.WriteLine($"【数据库】：{ConfigId},【SQL语句】：{sql },{GetParams(parm)}");
                        }
                    };
                    //SQL执行完
                    db.GetConnectionScope(ConfigId).Aop.OnLogExecuted = (sql, parm) =>
                    {
                        if (db.Ado.SqlExecutionTime.TotalMilliseconds > 600000)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            LogHelper.Info($"超时10Min,【操作时间】：{db.Ado.SqlExecutionTime.TotalMilliseconds},【数据库】：{ConfigId},【SQL语句】：{sql },{GetParams(parm)}");
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

        private static async Task careSparkServerAsync(string urls)
        {
            tokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                tokenSource.Cancel();
            };

            Task.Run(async () => await StartProcessAsync(tokenSource.Token, urls));
        }



        private static async Task StartProcessAsync(CancellationToken cancellationToken,string urls)
        {
            // 获取jdk根目录
            string rootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var filePath = Path.Combine(new string[] {
                rootFolder,
                "JavaHome",
                urls == "http://localhost:8700"?"windows":"linux",
                "bin",
                "java"
            });

            var commandPath = Path.Combine(new string[] {
                rootFolder,
                "JavaHome",
                "sparkExecuter-1.0.0.jar"
            });
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = $"-jar {commandPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            process.Exited += (s, e) =>
            {
                Console.WriteLine("The process exited with code " + process.ExitCode);
                // automatically restart the process if it exits unexpectedly
                if (!cancellationToken.IsCancellationRequested && process.ExitCode != 0)
                {
                    Console.WriteLine("Restarting the process...");
                    Thread.Sleep(10000);
                    Task.Run(async () => await StartProcessAsync(cancellationToken, urls));
                }
            };

            try
            {
                Console.WriteLine("Starting the process...");
                process.Start();
                var reader = process.StandardOutput;
                while (!reader.EndOfStream)
                {
                    Console.WriteLine(reader.ReadLine());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting the process: " + ex.Message);
                tokenSource.Cancel();
            }

            await Task.CompletedTask;
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
}
