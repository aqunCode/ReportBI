using Bi.Core.Models;
using Bi.Core.Extensions;
using Bi.Entities.Input;
using Bi.Services.IService;
using MagicOnion;
using Bi.Entities.Entity;
using System.Linq.Expressions;

namespace Bi.Services.Service;
//名称空间之中引用的名称空间代表优先使用依赖
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Bi.Core.Helpers;
using System.Net;
using Newtonsoft.Json;
using System.Data;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Dapper;
using SqlSugar;
using System.Linq;
using Bi.Core.Const;
using static MongoDB.Driver.WriteConcern;

public class DataSourceServices : IDataSourceServices {
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;
    /// <summary>
    /// 数据库处理服务
    /// </summary>
    private IDbEngineServices dbEngine;

    /// <summary>
    /// 构造函数
    /// </summary>
    public DataSourceServices(ISqlSugarClient _sqlSugarClient,
                                IDbEngineServices dbEngineService
                                ) {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
        dbEngine = dbEngineService;
    }

    public async UnaryResult<double> addAsync(DataSourceInput input) {
        var inputentitys = await repository.Queryable<DataSource>().Where(x => x.SourceCode == input.SourceCode).ToListAsync();
        if(inputentitys.Any())
            return BaseErrorCode.PleaseDoNotAddAgain;

        var entity = input.MapTo<DataSource>();
        entity.Create(input.CurrentUser);
        return await repository.Insertable(entity).ExecuteCommandAsync();
    }

    public async UnaryResult<PageEntity<IEnumerable<DataSource>>> getEntityListAsync(PageEntity<DataSourceInput> inputs) 
    {
        
        var condition = this.GetEntityExpression(inputs.Data);
        //分页查询
        RefAsync<int> total = 0;
        var data = await repository.Queryable<DataSource>()
            .WhereIF(
                !inputs.Data.SourceType.IsNullOrEmpty(),
                x => x.SourceType == inputs.Data.SourceType)
            .WhereIF(
                !inputs.Data.SourceCode.IsNullOrEmpty(),
                x => x.SourceCode.Contains(inputs.Data.SourceCode))
            .WhereIF(
                !inputs.Data.SourceName.IsNullOrEmpty(),
                x => x.SourceName.Contains(inputs.Data.SourceName))
            .WhereIF(
                true,
                x => x.DeleteFlag == 0)
            .OrderBy(x => inputs.OrderField, inputs.Ascending ? OrderByType.Asc : OrderByType.Desc)
            .ToPageListAsync(inputs.PageIndex, inputs.PageSize, total);

        // 此处遍历链接串加密
        foreach(var item in data)
        {
            item.SourceConnect = encrypt(item.SourceConnect);
        }
        return new PageEntity<IEnumerable<DataSource>> {
            PageIndex = inputs.PageIndex,
            Ascending = inputs.Ascending,
            PageSize = inputs.PageSize,
            OrderField = inputs.OrderField,
            Total = total,
            Data = data
        };

    }
    private Expression<Func<DataSource, bool>> GetEntityExpression(DataSourceInput input) {
        return LinqExtensions.True<DataSource>()
                .WhereIf(
                    !input.SourceType.IsNullOrEmpty(),
                    x => x.SourceType == input.SourceType)
                .WhereIf(
                    !input.SourceCode.IsNullOrEmpty(),
                    x => x.SourceCode.Contains(input.SourceCode))
                .WhereIf(
                    !input.SourceName.IsNullOrEmpty(),
                    x => x.SourceName.Contains(input.SourceName))
                .WhereIf(
                    true,
                    x => x.DeleteFlag == 0);
    }

    public async UnaryResult<double> ModifyAsync(DataSourceInput input) {
        var entitys = await repository.Queryable<DataSource>().Where(x => x.SourceCode == input.SourceCode).ToListAsync();
        DataSource dataSource;
        if (!entitys.Any())
            return BaseErrorCode.InvalidEncode;
        else
            dataSource = entitys.First();
        var entity = input.MapTo<DataSource>();
        entity.Modify(dataSource.Id, input.CurrentUser);
        // 密码解密
        entity.SourceConnect = unEncrypt(entity.SourceConnect);
        //ConcurrentBag<(IRepository, string)> value;
        //_ = CustomCache.keyValuePairs.TryRemove(input.sourceCode, out value);
        return await repository.Updateable(entity).ExecuteCommandAsync();
    }

    public async UnaryResult<double> deleteAsync(DataSourceDelete input) {
        int i = -1;
        foreach(String sourceCode in input.sourceCode) {
            var entitys = await repository.Queryable<DataSource>().Where(x => x.SourceCode == sourceCode).ToListAsync();
            if(!entitys.Any()) {
                continue;
            } else {
                var entity = new DataSource {
                    SourceCode = sourceCode,
                    DeleteFlag = 1
                };
                entity.Modify(entitys.First().Id, input.CurrentUser);
                await repository.Updateable(entitys.First()).ExecuteCommandAsync();
                i++;
            }
        }
        return i + 1;
    }

    public async UnaryResult<double> testConnection(DataSourceInput input) {
        if(input.SourceType == "http") {
            var result = await testHttp(input);
            if((int)result.Item2 == 200 && result.Item1 != null) {
                return BaseErrorCode.SourceConnectSuccess;
            } else {
                return BaseErrorCode.Fail;
            }
        }

        if (input.SourceType == "Spark")
        {
            var result = await testDriver(new DataCollectDBTest
            {
                SourceType = input.SourceType,
                SourceConnect = unEncrypt(input.SourceConnect),
                DynSql = "select 1"
            });
            if ((int)result.Item2 == 200 && result.Item1 != null)
            {
                return BaseErrorCode.SourceConnectSuccess;
            }
            else
            {
                return BaseErrorCode.Fail;
            }
        }

        var tmp = await testDB(
            new DataCollectDBTest {
                SourceType = input.SourceType,
                SourceConnect = unEncrypt(input.SourceConnect),
                DynSql = null
            });

        if(tmp.Item1 != null)
            return BaseErrorCode.SourceConnectSuccess;
        return BaseErrorCode.Fail;
    }
    // 这个方法用于连接spark驱动，并导出数据
    private async Task<(String, HttpStatusCode)> testExportDriver(DataCollectDBTest input)
    {

        /*JObject content = new JObject(
                new JProperty("dynamicSql", input.dynSql),
                new JProperty("SparkConnect", input.sourceConnect)
            );*/

        /*var queryStringParameters = new Dictionary<string, string>
        {
            { "dynamicSql", input.dynSql },
            { "SparkConnect", input.sourceConnect }
        };
        var queryString = new FormUrlEncodedContent(queryStringParameters);*/
        //string uriParam = $"?dynamicSql={input.dynSql}&SparkConnect={input.sourceConnect}";
        /*var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("dynamicSql", input.dynSql),
            new KeyValuePair<string, string>("SparkConnect", input.sourceConnect),
        });*/
        var templatePath = AppDomain.CurrentDomain.BaseDirectory + $"excel/";
        var fileName = $"{Guid.NewGuid()}.csv";
        if (!Directory.Exists(templatePath))
        {
            Directory.CreateDirectory(templatePath);
        }

        string url = "http://localhost:8104/spark/export";
        string filePath = templatePath + fileName;
        HttpStatusCode code =  await DownloadFileAsync(url, filePath, input);

        return (fileName, code);
    }

    public async Task<HttpStatusCode> DownloadFileAsync(string requestUri, string filePath, DataCollectDBTest input)
    {
        int count = 10000;
        // 创建 HttpClient 实例，并设置请求超时时间为 8 小时
        using (var httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromHours(8);

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var dynsql = input.DynSql.Replace("\n", " ").Replace("\r", " ");
            request.Headers.Add("x-api-key", "your-api-key");
            request.Headers.Add("dynamicSql", dynsql);
            request.Headers.Add("SparkConnect", input.SourceConnect);

            request.RequestUri = new Uri(request.RequestUri.ToString());


            // 创建请求的内容对象
            // var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            // 发送请求并获取响应流
            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                // 打开文件以供写入
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    // 遍历响应流并将它写入到文件中
                    var buffer = new byte[81920];  // 读取的字节数
                    var totalBytesRead = 0L;       // 已经读取的字节数
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L; // 总字节数 (单位: byte)，如果无法获取，则为 -1
                    int stage = 1000000;
                    while (true)
                    {
                        // 从响应流中读取数据
                        var bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead == 0) break; // EOF

                        // 将读取的数据写入到文件中
                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                        // 更新总字节数和已经读取的字节数
                        totalBytesRead += bytesRead;

                        // 统计已经下载的文件的百分比
                        var progress = totalBytes == -1 ? "N/A" : Math.Round((double)totalBytesRead / totalBytes * 100, 2).ToString();
                        if (stage-- <= 0)
                        {
                            stage = 1000000;
                            Console.WriteLine($"Downloaded: {totalBytesRead} bytes ({progress}%).");
                        }
                        // 输出下载进度和已经下载的字节数
                        
                    }
                    Console.WriteLine($"Downloaded: {totalBytesRead}");
                }
            }

            /*// 发送请求并获取响应流
            using (var responseStream = await (await httpClient.PostAsync(requestUri, content)).Content.ReadAsStreamAsync())
            {
                // 创建文件流，并使用流的方式将响应的信息写入到文件
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var streamReader = new StreamReader(responseStream, Encoding.Default))
                    {
                        using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            // 按行读取 CSV 文件并将每行信息写入文件

                            while (!streamReader.EndOfStream)
                            {
                                count--;
                                var line = await streamReader.ReadLineAsync();
                                await streamWriter.WriteLineAsync(line);
                                if (count <= 0)
                                {
                                    streamWriter.Flush();
                                    fileStream.Flush();
                                    count = 10000;
                                }
                            }
                        }
                    }
                }
            }
*/
            // 返回响应状态码
        }
        return HttpStatusCode.OK;
    }

    public async Task<HttpStatusCode> DownloadFileAsync2(string requestUri, string filePath, JObject requestBody)
    {
        var httpClient = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        var responseStream = await (await httpClient.PostAsync(requestUri, content)).Content.ReadAsStreamAsync();

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, len);
            }
        }
        return HttpStatusCode.OK;
    }

    // 这个方法用于连接spark驱动，并调用数据
    private async Task<(String, HttpStatusCode)> testDriver(DataCollectDBTest input)
    {
        (String, HttpStatusCode) result;

        JObject httpBody = new JObject(
                new JProperty("dynamicSql", input.DynSql),
                new JProperty("SparkConnect", input.SourceConnect)
            );
        /*@delegate: x => {
                            x.Timeout = TimeSpan.FromSeconds(2400);
                        },*/
        result = await HttpClientHelper.PostAsync(
                "http://localhost:8104/spark/select",
                httpBody.ToString(),
                @delegate: x => {
                    x.Timeout = TimeSpan.FromSeconds(28800);
                },
                httpClientName: "default");
        if (result.Item1.IsNullOrEmpty())
        {
            result = ("查询结果为空",HttpStatusCode.NoContent);
        }
        return result;
    }

    public async UnaryResult<(String, HttpStatusCode)> testHttp(DataSourceInput httpTest) {
        (String, HttpStatusCode) result;
        if(httpTest.HttpWay.ToLower() == "post") {
            IEnumerable<JProperty> properties = JObject.Parse(httpTest.HttpHeader).Properties();
            HttpContent httpContent = new StringContent(httpTest.HttpBody);
            foreach(JProperty property in properties) {
                httpContent.Headers.Remove(property.Name);
                httpContent.Headers.Add(property.Name, property.Value.ToString());
            }
            result = await HttpClientHelper.PostAsync(
                    httpTest.HttpAddress,
                    httpContent,
                    @delegate: x => {
                        x.Timeout = TimeSpan.FromSeconds(1200);
                            //x.DefaultRequestHeaders.Add("Content-Type", "application/json");
                        },
                    httpClientName: "default");
            //result = await client.PostAsync(input.httpAddress,httpContent);
        } else if(httpTest.HttpWay.ToLower() == "get") {
            result = await HttpClientHelper.GetAsync(
                httpTest.HttpAddress,
                JsonConvert.DeserializeObject<Dictionary<String, String>>(httpTest.HttpBody),
                headers: JsonConvert.DeserializeObject<Dictionary<String, String>>(httpTest.HttpHeader),
                @delegate: x => {
                    x.Timeout = TimeSpan.FromSeconds(1200);
                },
                httpClientName: "default");
        } else {
            result = (null, HttpStatusCode.BadRequest);
        }
        return result;
    }

    public async UnaryResult<(DataTable, long, long)> testDB(DataCollectDBTest dbTest) {
        if(dbTest.LimitEnd - dbTest.LimitStart <= 0) {
            dbTest.LimitStart = 0;
            dbTest.LimitEnd = 500;
        }
        else
        {
            dbTest.LimitStart--;
        }

        (SqlSugarScope, string) items = getRepositoryCache(dbTest.SourceCode, dbTest.SourceType, dbTest.SourceConnect);

        if (items.Item1 == null)
            return (null, 0, 0);

        var sql = dbTest.DynSql ?? items.Item2;

        // sql 二次加工 最父格过滤，汇总，排序
        sql = sqlRework(sql, dbTest);
        
        DataTable data = null;
        long total = 0;
        if (dbTest.SearchAll || dbTest.Export)
        {
            // 设定执行20分钟为上限时间
            if (dbTest.SourceType == "Spark")
            {
                var res = await Task.Run(() => testDriver(new DataCollectDBTest
                {
                    SourceConnect = dbTest.SourceConnect,
                    DynSql = sql
                }));

                if (res.Item2 == HttpStatusCode.OK)
                    data = JsonConvert.DeserializeObject<DataTable>(res.Item1);
                else
                    throw new Exception(res.Item1);
            }
            else
            {
                var task = Task.Run(() => items.Item1.Ado.GetDataTable(sql));
                if (await Task.WhenAny(task, Task.Delay(2400000)) == task)
                    data = task.Result;
                else
                    throw new TimeoutException("sql执行时间超出40分钟");
            }

            if (data == null)
            {
                throw new Exception("sql执行时间超出40分钟");
            }else if (dbTest.Export && total >= 1000001) {
                throw new Exception("导出上限为1000000");
            }
            total = data.Rows.Count;
        }
        else
        {
            sql = dbEngine.sqlPageRework(sql, dbTest.LimitStart ,dbTest.LimitEnd, dbTest.SourceType);

            if (dbTest.SourceType == "Spark")
            {
                var res = await testDriver(new DataCollectDBTest
                {
                    SourceConnect = dbTest.SourceConnect,
                    DynSql = sql
                });
                
                if (res.Item2 == HttpStatusCode.OK)
                    return (JsonConvert.DeserializeObject<DataTable>(res.Item1), -1, -1);
                else
                    throw new Exception(res.Item1);
            }
            else
                await Task.Run(() =>
               {
                    data = items.Item1.Ado.GetDataTable(sql);
                    if(dbTest.SourceType == "SqlServer")
                   {
                       DataTable dtResult = data.Clone();
                       // 解锁 DataTable 
                       DataTableHelper.unLockReadOnly(dtResult);
                       var startIndex = data.Rows.Count > (dbTest.LimitEnd - dbTest.LimitStart) ? 
                                            data.Rows.Count - (dbTest.LimitEnd - dbTest.LimitStart) : 0;
                       // 遍历获取后十笔数据
                       for (int i = startIndex; i < data.Rows.Count; i++)
                           dtResult.Rows.Add(data.Rows[i].ItemArray);
                       data = dtResult;
                   }

               });
        }
        // 释放查询缓存
        SqlMapper.PurgeQueryCache();

        if (dbTest.SourceType == "Spark")
            items.Item1 = null;
        else
            items.Item1.Dispose();

        if (data != null)
            return (data, -1, -1);
        return (null, 0, 0);
    }

    /// <summary>
    /// sql 二次加工 最父格过滤，分组，排序
    /// </summary>
    private string sqlRework(string sql, DataCollectDBTest dbTest)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("select * from (");
        sb.Append("{sql}");
        sb.Append(") t ");
        int initLength = sb.Length;
        // 1,最父格筛选值
        if (dbTest.SqlFiltration.IsNotNullOrEmpty())
        {
            sb.Append(" where ");
            sb.Append(dbTest.SqlFiltration);
        }
        //2 ,汇总计算
        if (dbTest.GroupList != null && dbTest.GroupList.Count > 0)
        {
            StringBuilder sbGroup = new StringBuilder();
            for (int i = 0; i < dbTest.GroupList.Count - 2; i++)
            {
                sbGroup.Append(dbTest.GroupList[i]);
                sbGroup.Append(',');
            }
            string groupBy = sbGroup.ToString().Substring(0, sbGroup.Length - 1);
            switch (dbTest.GroupList[dbTest.GroupList.Count - 1])
            {
                case "sum":
                    sbGroup.Append("sum(");
                    break;
                case "avg":
                    sbGroup.Append("avg(");
                    break;
                case "max":
                    sbGroup.Append("max(");
                    break;
                case "min":
                    sbGroup.Append("min(");
                    break;
                case "count":
                    sbGroup.Append("count(");
                    break;
                case "countDistinct":
                    sbGroup.Append("count(DISTINCT ");
                    break;
                default:
                    sbGroup.Append("count(");
                    break;
            }
            sbGroup.Append(dbTest.GroupList[dbTest.GroupList.Count - 2]);
            sbGroup.Append(") ");
            sbGroup.Append(dbTest.GroupList[dbTest.GroupList.Count - 2]);
            sb.Remove(7, 1);
            sb.Insert(7, sbGroup.ToString());
            sb.Append(" group by ");
            sb.Append(groupBy);
        }
        // 开始排序
        if (dbTest.OrderArr != null && dbTest.OrderArr.Count > 0)
        {
            sb.Append(" order by ");
            foreach (var item in dbTest.OrderArr)
            {
                sb.Append(item.Split('-')[0]);
                sb.Append(' ');
                sb.Append(item.Split('-')[1]);
                sb.Append(',');
            }
            sb = sb.Remove(sb.Length - 1, 1);
        }

        if(sb.Length == initLength)
        {
            return sql;
        }
        else
        {
            return sb.Replace("{sql}", sql).ToString();
        }
    }

    

    private string unEncrypt(string str)
    {
        if (string.IsNullOrEmpty(str) || !str.StartsWith("encrypt-"))
        {
            return str;
        }
        String[] arr = str.Split('-');
        byte[] key = Convert.FromHexString(arr[1]);
        byte[] iv = Convert.FromHexString(arr[2]);
        byte[] encrypted = Convert.FromHexString(arr[3]);
        // Decrypt the bytes to a string.
        return DecryptStringFromBytes_Aes(encrypted, key, iv);
    }

    private string encrypt(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }
        StringBuilder res = new StringBuilder();
        // Create a new instance of the Aes
        // class.  This generates a new key and initialization
        // vector (IV).
        using (Aes myAes = Aes.Create())
        {
            // Encrypt the string to an array of bytes.
            byte[] encrypted = EncryptStringToBytes_Aes(str, myAes.Key, myAes.IV);
            res.Append("encrypt-");
            res.Append(Convert.ToHexString(myAes.Key));
            res.Append("-");
            res.Append(Convert.ToHexString(myAes.IV));
            res.Append("-");
            res.Append(Convert.ToHexString(encrypted));
        }
        return res.ToString();
    }

    private byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");
        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }

    public async UnaryResult<(IDataReader,string)> testDB(DataReaderDBTest dbTest)
    {
        (SqlSugarScope, string) items = dbEngine.GetRepository(dbTest.SourceType, dbTest.SourceConnect);
        // 获取数据库链接
        //(IRepository, string) items = getRepositoryCache(dbTest.sourceCode, dbTest.sourceType, dbTest.sourceConnect);
        

        if (items.Item1 == null)
            return (null,null);

        var sql = dbTest.DynSql ?? items.Item2;

        IDataReader data = null;
        string filePath = "";
        // 设定执行20分钟为上限时间
        // await Task.Run();
        /*bool flag = new System.Threading.Tasks.TaskFactory().StartNew(
            () => { data = items.Item1.Ado.GetDataReader(sql); }
        ).Wait(2400000);
        if (!flag)
        {
            throw new Exception("sql执行时间超出40分钟");
        }*/
        if (dbTest.SourceType == "Spark")
        {
            var res = await Task.Run(() => testExportDriver(new DataCollectDBTest
            {
                SourceConnect = dbTest.SourceConnect,
                DynSql = sql
            }));

            if (res.Item2 == HttpStatusCode.OK)
                filePath = res.Item1;
            else
                throw new Exception("查询结果失败");
        }
        else
        {
            var task = Task.Run(() => items.Item1.Ado.GetDataReader(sql));
            if (await Task.WhenAny(task, Task.Delay(2400000)) == task)
                data = task.Result;
            else
                throw new TimeoutException("sql执行时间超出40分钟");
        }

        // 释放查询缓存
        SqlMapper.PurgeQueryCache();
        //DataReader 会一直占用链接，所以不能归还
        //items.Item1.Dispose();
        //归还数据库链接
        //backRepositoryCache(dbTest.sourceCode, items.Item1,items.Item2);

        return (data,filePath);
    }
    public async UnaryResult<int> testDB(DataCountDBTest dbTest)
    {
        // 获取数据库链接
        (SqlSugarScope, string) items = getRepositoryCache(dbTest.SourceCode, dbTest.SourceType, dbTest.SourceConnect);

        if (items.Item1 == null)
            return -1;

        var sql = dbTest.DynSql ?? items.Item2;

        sql = $" select count(*) counts from ({sql}) t";

        DataTable data = null;
        // 设定执行20分钟为上限时间
        if (dbTest.SourceType == "Spark")
        {
            var res = await Task.Run(() => testDriver(new DataCollectDBTest
            {
                SourceConnect = dbTest.SourceConnect,
                DynSql = sql
            }));

            if (res.Item2 == HttpStatusCode.OK)
                data = JsonConvert.DeserializeObject<DataTable>(res.Item1);
            else
                throw new Exception(res.Item1);
        }
        else
        {
            var task = Task.Run(() => items.Item1.Ado.GetDataTable(sql));
            if (await Task.WhenAny(task, Task.Delay(2400000)) == task)
                data = task.Result;
            else
                throw new TimeoutException("sql执行时间超出40分钟");
        }

        // 释放查询缓存
        SqlMapper.PurgeQueryCache();

        if (dbTest.SourceType == "Spark")
            items.Item1 = null;
        else
            items.Item1.Dispose();

        return Convert.ToInt32(data.Rows[0][0]);
    }
    

    /// <summary>
    /// 从缓存中获取数据库链接
    /// </summary>
    private (SqlSugarScope, string) getRepositoryCache(string sourceCode, string sourceType, string sourceConnect)
    {
        return  dbEngine.GetRepository(sourceType, sourceConnect);
    }

    

    public class CustomCache {
        public static ConcurrentDictionary<string, ConcurrentBag<(SqlSugarScope, string)>> keyValuePairs = new();
    }

}

