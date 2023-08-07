﻿using Bi.Core.Extensions;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// 文件帮助类
    /// </summary>
    public static class FileHelper
    {
        #region 日志写锁
        /// <summary>
        /// 日志写锁
        /// </summary>
        private static readonly ReaderWriterLockSlim logWriteLock = new ReaderWriterLockSlim();
        #endregion

        #region 创建文件
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileBytes"></param>
        public static void Create(string filePath, params byte[] fileBytes)
        {
            using var fs = File.Create(filePath);
            fs.Write(fileBytes);
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileStream"></param>
        public static void Create(string filePath, Stream fileStream)
        {
            using (fileStream)
            {
                using var fs = File.Create(filePath);
                var buffer = new byte[2048];
                var count = 0;
                //每次读取2kb数据，然后写入文件
                while ((count = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fs.Write(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileBytes"></param>
        public static async Task CreateAsync(string filePath, params byte[] fileBytes)
        {
            using var fs = File.Create(filePath);
            await fs.WriteAsync(fileBytes, 0, fileBytes.Length);
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileStream"></param>
        public static async Task CreateAsync(string filePath, Stream fileStream)
        {
            using (fileStream)
            {
                using var fs = File.Create(filePath);
                var buffer = new byte[2048];
                var count = 0;
                //每次读取2kb数据，然后写入文件
                while ((count = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    await fs.WriteAsync(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 读取嵌入资源创建指定文件
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="manifestResourcePath">嵌入资源路径</param>
        /// <param name="filePath">文件路径</param>
        public static void CreateFileFromManifestResource(Assembly assembly, string manifestResourcePath, string filePath)
        {
            if (!File.Exists(filePath))
            {
                //读取嵌入资源
                using var stream = assembly.GetManifestResourceStream(manifestResourcePath);
                using var fs = File.Create(filePath);
                var buffer = new byte[2048];
                var count = 0;
                //每次读取2kb数据，然后写入文件
                while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fs.Write(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 读取嵌入资源创建指定文件
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="manifestResourcePath">嵌入资源路径</param>
        /// <param name="filePath">文件路径</param>
        public static async Task CreateFileFromManifestResourceAsync(Assembly assembly, string manifestResourcePath, string filePath)
        {
            if (!File.Exists(filePath))
            {
                //读取嵌入资源
                using var stream = assembly.GetManifestResourceStream(manifestResourcePath);
                using var fs = File.Create(filePath);
                var buffer = new byte[2048];
                var count = 0;
                //每次读取2kb数据，然后写入文件
                while ((count = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await fs.WriteAsync(buffer, 0, count);
                }
            }
        }
        #endregion

        #region 获取文件
        #region 同步方法
        /// <summary>
        /// 续传获取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        public static void GetFile(string filePath, string fileName, bool isDeleteFile = false)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var buffer = new byte[10240];
                var dataToRead = fileStream.Length;
                long position = 0;
                if (HttpContextHelper.Current.Request.Headers["Range"].IsNotNullOrEmpty())
                {
                    HttpContextHelper.Current.Response.StatusCode = 206;
                    var range = HttpContextHelper.Current.Request.Headers["Range"].ToString().Replace("bytes=", "");
                    position = long.Parse(range.Substring(0, range.IndexOf("-")));
                }

                if (position != 0)
                    HttpContextHelper.Current.Response.Headers.Add("Content-Range", $"bytes {position}-{dataToRead - 1}/{dataToRead}");

                HttpContextHelper.Current.Response.Headers.Add("Content-Length", (dataToRead - position).ToString());
                HttpContextHelper.Current.Response.ContentType = "application/octet-stream";
                HttpContextHelper.Current.Response.Headers.Add("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(fileName))}");
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                HttpContextHelper.Current.Response.Cookies.Append("fileDownload", "true");
                fileStream.Position = position;
                dataToRead -= position;
                while (dataToRead > 0)
                {
                    var length = fileStream.Read(buffer, 0, 10240);
                    HttpContextHelper.Current.Response.Body.Write(buffer, 0, length);
                    HttpContextHelper.Current.Response.Body.Flush();
                    buffer = new byte[10240];
                    dataToRead -= length;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        ///  限速续传获取文件
        /// </summary>
        /// <param name="fileName">下载文件名</param>
        /// <param name="fullPath">带文件名下载路径</param>
        /// <param name="speed">每秒允许下载的字节数</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        /// <returns>返回是否成功</returns>
        public static void GetFile(string fileName, string fullPath, long speed, bool isDeleteFile = false)
        {
            try
            {
                var request = HttpContextHelper.Current.Request;
                var response = HttpContextHelper.Current.Response;
                using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var binaryReader = new BinaryReader(fileStream);
                response.Headers.Add("Accept-Ranges", "bytes");
                var fileLength = fileStream.Length;
                long startBytes = 0;
                var pack = 10240;  //10K bytes
                var sleep = (int)Math.Floor((double)(1000 * pack / speed)) + 1;
                if (request.Headers["Range"] != StringValues.Empty)
                {
                    response.StatusCode = 206;
                    var range = request.Headers["Range"].ToString().Split(new char[] { '=', '-' });
                    startBytes = Convert.ToInt64(range[1]);
                }
                response.Headers.Add("Content-Length", (fileLength - startBytes).ToString());
                if (startBytes != 0)
                {
                    response.Headers.Add("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                }
                response.Headers.Add("Connection", "Keep-Alive");
                response.ContentType = "application/octet-stream";
                response.Headers.Add("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName, Encoding.UTF8));
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                response.Cookies.Append("fileDownload", "true");
                binaryReader.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                var maxCount = Math.Floor((double)((fileLength - startBytes) / pack)) + 1;
                for (var i = 0d; i < maxCount; i++)
                {
                    response.Body.Write(binaryReader.ReadBytes(pack));
                    Thread.Sleep(sleep);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        /// 普通获取文件
        /// </summary>
        /// <param name="fileBytes">文件字节</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件类型【application/octet-stream、application/pdf、text/plain、text/html、application/zip、application/msword、application/vnd.ms-excel、application/vnd.ms-powerpoint、image/gif、image/png、image/jpg】</param>
        public static void GetFile(byte[] fileBytes, string fileName, string contentType)
        {
            try
            {
                HttpContextHelper.Current.Response.Headers.Add("Pragma", "public");
                HttpContextHelper.Current.Response.Headers.Add("Expires", "0");
                HttpContextHelper.Current.Response.Headers.Add("Cache-Control", "must-revalidate, pre-check=0");
                HttpContextHelper.Current.Response.Headers.Add("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(fileName, Encoding.UTF8)}");
                HttpContextHelper.Current.Response.Headers.Add("Content-Type", contentType);
                HttpContextHelper.Current.Response.Headers.Add("Content-Transfer-Encoding", "binary");
                HttpContextHelper.Current.Response.Headers.Add("Content-Length", fileBytes.Length.ToString());
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                HttpContextHelper.Current.Response.Cookies.Append("fileDownload", "true");
                HttpContextHelper.Current.Response.Body.Write(fileBytes);
                HttpContextHelper.Current.Response.Body.Flush();//输出缓存，此部不可少
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        /// 普通获取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件类型【application/octet-stream、application/pdf、text/plain、text/html、application/zip、application/msword、application/vnd.ms-excel、application/vnd.ms-powerpoint、image/gif、image/png、image/jpg】</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        public static void GetFile(string filePath, string fileName, string contentType, bool isDeleteFile = false)
        {
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                HttpContextHelper.Current.Response.Headers.Add("Pragma", "public");
                HttpContextHelper.Current.Response.Headers.Add("Expires", "0");
                HttpContextHelper.Current.Response.Headers.Add("Cache-Control", "must-revalidate, pre-check=0");
                HttpContextHelper.Current.Response.Headers.Add("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(fileName, Encoding.UTF8)}");
                HttpContextHelper.Current.Response.Headers.Add("Content-Type", contentType);
                HttpContextHelper.Current.Response.Headers.Add("Content-Transfer-Encoding", "binary");
                HttpContextHelper.Current.Response.Headers.Add("Content-Length", bytes.Length.ToString());
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                HttpContextHelper.Current.Response.Cookies.Append("fileDownload", "true");
                HttpContextHelper.Current.Response.Body.Write(bytes);
                HttpContextHelper.Current.Response.Body.Flush();//输出缓存，此部不可少
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="filePaths">文件路径数组</param>
        /// <param name="zipName">压缩文件名</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        /// <param name="password">压缩密码</param>
        public static void GetFileOfZip(string[] filePaths, string zipName, bool isDeleteFiles = false, string password = null)
        {
            try
            {
                if (filePaths?.Length > 0)
                {
                    HttpContextHelper.Current.Response.ContentType = "application/zip";
                    HttpContextHelper.Current.Response.Headers.Add("content-disposition", $"filename={zipName}");
                    //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                    HttpContextHelper.Current.Response.Cookies.Append("fileDownload", "true");
                    using var zipStream = new ZipOutputStream(HttpContextHelper.Current.Response.Body);

                    if (password.IsNotNullOrEmpty())
                        zipStream.Password = password;

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                    foreach (string file in filePaths)
                    {
                        using var fs = File.OpenRead(file);
                        var entry = new ZipEntry(Path.GetFileName(file)) { Size = fs.Length, IsUnicodeText = true };
                        //Setting the Size provides WinXP built-in extractor compatibility,
                        //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                        zipStream.PutNextEntry(entry);
                        var buffer = new byte[4096];
                        var count = 0;
                        while ((count = fs.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            zipStream.Write(buffer, 0, count);
                            HttpContextHelper.Current.Response.Body.Flush();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var file in filePaths)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="filePaths">文件路径数组</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        /// <param name="password">压缩密码</param>
        public static byte[] GetFileOfZip(string[] filePaths, bool isDeleteFiles = false, string password = null)
        {
            try
            {
                if (filePaths?.Length > 0)
                {
                    using var stream = new MemoryStream();
                    using var zipStream = new ZipOutputStream(stream);

                    if (password.IsNotNullOrEmpty())
                        zipStream.Password = password;

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                    foreach (string file in filePaths)
                    {
                        using var fs = File.OpenRead(file);
                        var entry = new ZipEntry(Path.GetFileName(file)) { Size = fs.Length, IsUnicodeText = true };
                        //Setting the Size provides WinXP built-in extractor compatibility,
                        //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                        zipStream.PutNextEntry(entry);
                        var buffer = new byte[4096];
                        var count = 0;
                        while ((count = fs.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            zipStream.Write(buffer, 0, count);
                            zipStream.Flush();
                        }
                    }
                    zipStream.Finish();
                    return stream.ToArray();
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var file in filePaths)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
            }
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 续传获取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        public static async Task GetFileAsync(string filePath, string fileName, bool isDeleteFile = false)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var buffer = new byte[10240];
                var dataToRead = fileStream.Length;
                long position = 0;
                if (HttpContextHelper.Current.Request.Headers["Range"] != StringValues.Empty)
                {
                    HttpContextHelper.Current.Response.StatusCode = 206;
                    var range = HttpContextHelper.Current.Request.Headers["Range"].ToString().Replace("bytes=", "");
                    position = long.Parse(range.Substring(0, range.IndexOf("-")));
                }

                if (position != 0)
                    HttpContextHelper.Current.Response.Headers.Add("Content-Range", $"bytes {position}-{dataToRead - 1}/{dataToRead}");

                HttpContextHelper.Current.Response.Headers.Add("Content-Length", (dataToRead - position).ToString());
                HttpContextHelper.Current.Response.ContentType = "application/octet-stream";
                HttpContextHelper.Current.Response.Headers.Add("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(fileName))}");
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                HttpContextHelper.Current.Response.Cookies.Append("fileDownload", "true");
                fileStream.Position = position;
                dataToRead -= position;
                while (dataToRead > 0)
                {
                    var length = await fileStream.ReadAsync(buffer, 0, 10240);
                    await HttpContextHelper.Current.Response.Body.WriteAsync(buffer, 0, length);
                    await HttpContextHelper.Current.Response.Body.FlushAsync();
                    buffer = new byte[10240];
                    dataToRead -= length;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        ///  限速续传获取文件
        /// </summary>
        /// <param name="fileName">下载文件名</param>
        /// <param name="fullPath">带文件名下载路径</param>
        /// <param name="speed">每秒允许下载的字节数</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        /// <returns>返回是否成功</returns>
        public static async Task GetFileAsync(string fileName, string fullPath, long speed, bool isDeleteFile = false)
        {
            try
            {
                var request = HttpContextHelper.Current.Request;
                var response = HttpContextHelper.Current.Response;
                using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var binaryReader = new BinaryReader(fileStream);
                response.Headers.Add("Accept-Ranges", "bytes");
                var fileLength = fileStream.Length;
                long startBytes = 0;
                var pack = 10240;  //10K bytes
                var sleep = (int)Math.Floor((double)(1000 * pack / speed)) + 1;
                if (request.Headers["Range"] != StringValues.Empty)
                {
                    response.StatusCode = 206;
                    var range = request.Headers["Range"].ToString().Split(new char[] { '=', '-' });
                    startBytes = Convert.ToInt64(range[1]);
                }
                response.Headers.Add("Content-Length", (fileLength - startBytes).ToString());
                if (startBytes != 0)
                {
                    response.Headers.Add("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                }
                response.Headers.Add("Connection", "Keep-Alive");
                response.ContentType = "application/octet-stream";
                response.Headers.Add("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName, Encoding.UTF8));
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                response.Cookies.Append("fileDownload", "true");
                binaryReader.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                var maxCount = Math.Floor((double)((fileLength - startBytes) / pack)) + 1;
                for (var i = 0d; i < maxCount; i++)
                {
                    var bytes = binaryReader.ReadBytes(pack);
                    await response.Body.WriteAsync(bytes, 0, bytes.Length);
                    Thread.Sleep(sleep);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        /// 普通获取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件类型【application/octet-stream、application/pdf、text/plain、text/html、application/zip、application/msword、application/vnd.ms-excel、application/vnd.ms-powerpoint、image/gif、image/png、image/jpg】</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        public static async Task GetFileAsync(string filePath, string fileName, string contentType, bool isDeleteFile = false)
        {
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                HttpContextHelper.Current.Response.Headers.Add("Pragma", "public");
                HttpContextHelper.Current.Response.Headers.Add("Expires", "0");
                HttpContextHelper.Current.Response.Headers.Add("Cache-Control", "must-revalidate, pre-check=0");
                HttpContextHelper.Current.Response.Headers.Add("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(fileName, Encoding.UTF8)}");
                HttpContextHelper.Current.Response.Headers.Add("Content-Type", contentType);
                HttpContextHelper.Current.Response.Headers.Add("Content-Transfer-Encoding", "binary");
                HttpContextHelper.Current.Response.Headers.Add("Content-Length", bytes.Length.ToString());
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                HttpContextHelper.Current.Response.Cookies.Append("fileDownload", "true");
                await HttpContextHelper.Current.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                await HttpContextHelper.Current.Response.Body.FlushAsync();//输出缓存，此部不可少
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        /// 普通获取文件
        /// </summary>
        /// <param name="fileBytes">文件字节</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件类型【application/octet-stream、application/pdf、text/plain、text/html、application/zip、application/msword、application/vnd.ms-excel、application/vnd.ms-powerpoint、image/gif、image/png、image/jpg】</param>
        public static async Task GetFileAsync(byte[] fileBytes, string fileName, string contentType)
        {
            try
            {
                HttpContextHelper.Current.Response.Headers.Add("Pragma", "public");
                HttpContextHelper.Current.Response.Headers.Add("Expires", "0");
                HttpContextHelper.Current.Response.Headers.Add("Cache-Control", "must-revalidate, pre-check=0");
                HttpContextHelper.Current.Response.Headers.Add("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(fileName, Encoding.UTF8)}");
                HttpContextHelper.Current.Response.Headers.Add("Content-Type", contentType);
                HttpContextHelper.Current.Response.Headers.Add("Content-Transfer-Encoding", "binary");
                HttpContextHelper.Current.Response.Headers.Add("Content-Length", fileBytes.Length.ToString());
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                HttpContextHelper.Current.Response.Cookies.Append("fileDownload", "true");
                await HttpContextHelper.Current.Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
                await HttpContextHelper.Current.Response.Body.FlushAsync();//输出缓存，此部不可少
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="filePaths">文件路径数组</param>
        /// <param name="zipName">压缩文件名</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        /// <param name="password">压缩密码</param>
        public static async Task GetFileOfZipAsync(string[] filePaths, string zipName, bool isDeleteFiles = false, string password = null)
        {
            try
            {
                if (filePaths?.Length > 0)
                {
                    HttpContextHelper.Current.Response.ContentType = "application/zip";
                    HttpContextHelper.Current.Response.Headers.Add("content-disposition", $"filename={zipName}");
                    //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效
                    HttpContextHelper.Current.Response.Cookies.Append("fileDownload", "true");
                    using var zipStream = new ZipOutputStream(HttpContextHelper.Current.Response.Body);

                    if (password.IsNotNullOrEmpty())
                        zipStream.Password = password;

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                    foreach (string file in filePaths)
                    {
                        using var fs = File.OpenRead(file);
                        var entry = new ZipEntry(Path.GetFileName(file)) { Size = fs.Length, IsUnicodeText = true };
                        //Setting the Size provides WinXP built-in extractor compatibility,
                        //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                        zipStream.PutNextEntry(entry);
                        var buffer = new byte[4096];
                        var count = 0;
                        while ((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await zipStream.WriteAsync(buffer, 0, count);
                            await HttpContextHelper.Current.Response.Body.FlushAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var file in filePaths)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
                HttpContextHelper.Current.Response.Body.Close();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="filePaths">文件路径数组</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        /// <param name="password">压缩密码</param>
        public static async Task<byte[]> GetFileOfZipAsync(string[] filePaths, bool isDeleteFiles = false, string password = null)
        {
            try
            {
                if (filePaths?.Length > 0)
                {
                    using var stream = new MemoryStream();
                    using var zipStream = new ZipOutputStream(stream);

                    if (password.IsNotNullOrEmpty())
                        zipStream.Password = password;

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                    foreach (string file in filePaths)
                    {
                        using var fs = File.OpenRead(file);
                        var entry = new ZipEntry(Path.GetFileName(file)) { Size = fs.Length, IsUnicodeText = true };
                        //Setting the Size provides WinXP built-in extractor compatibility,
                        //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                        zipStream.PutNextEntry(entry);
                        var buffer = new byte[4096];
                        var count = 0;
                        while ((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await zipStream.WriteAsync(buffer, 0, count);
                            await zipStream.FlushAsync();
                        }
                    }
                    zipStream.Finish();
                    return stream.ToArray();
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var file in filePaths)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
            }
        }
        #endregion
        #endregion

        #region 读取文件
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath">物理路径</param>
        /// <returns>string</returns>
        public static string ReadFile(string filePath)
        {
            var sb = new StringBuilder();
            if (File.Exists(filePath))
            {
                using var sr = new StreamReader(filePath);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath">物理路径</param>
        /// <returns>string</returns>
        public static async Task<string> ReadFileAsync(string filePath)
        {
            var sb = new StringBuilder();
            if (File.Exists(filePath))
            {
                using var sr = new StreamReader(filePath);
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="dir">目录实例数组</param>
        /// <returns>string</returns>
        public static string GetDirectoryInfo(DirectoryInfo[] dir)
        {
            var result = new StringBuilder();
            if (dir.IsNotNull())
            {
                var comparer = new IComparerHelper<string>((x, y) =>
                   x.IsValidDecimal() && y.IsValidDecimal()
                   ? decimal.Compare(x.ToDecimal(), y.ToDecimal())
                   : string.Compare(x, y));

                var query = (from a in dir
                             let files = a.GetFiles().Select(o => o.Name)
                             let dirs = a.GetDirectories()
                             select new
                             {
                                 dirName = a.Name,
                                 filesName = files,
                                 dirsName = dirs
                             })
                             .OrderBy(o => o.dirName)
                             .ToList();
                result.Append("[");
                query.ForEach(o =>
                {
                    result.Append("{")
                          .Append($"\"dirName\":\"{o.dirName}\",")
                          .Append($"\"filesName\":{o.filesName.OrderBy(s => s.Contains(".") ? s.Substring(0, s.LastIndexOf(".")) : s, comparer).ToJson()},")
                          .Append($"\"childrensDirInfo\":{GetDirectoryInfo(o.dirsName)}")
                          .Append("},");
                });
                result = query.Count > 0 ? result.Remove(result.Length - 1, 1) : result;
                result.Append("]");
            }
            return result.ToString();
        }

        /// <summary>
        /// 获取指定路径下的文件目录和文件信息
        /// </summary>
        /// <param name="path">指定路径</param>
        /// <returns>string</returns>
        public static string GetDirectoryInfo(string path)
        {
            var result = new StringBuilder();
            if (Directory.Exists(path))
            {
                var comparer = new IComparerHelper<string>((x, y) =>
                   x.IsValidDecimal() && y.IsValidDecimal()
                   ? decimal.Compare(x.ToDecimal(), y.ToDecimal())
                   : string.Compare(x, y));

                var di = new DirectoryInfo(path);
                var files = di.GetFiles();
                var dirs = di.GetDirectories();
                result.Append("{")
                      .Append($"\"dirName\":\"{path.Substring("\\")}\",")
                      .Append($"\"filesName\":{files?.Select(o => o.Name).OrderBy(o => o.Contains(".") ? o.Substring(0, o.LastIndexOf(".")) : o, comparer).ToJson()},")
                      .Append($"\"childrensDirInfo\":{GetDirectoryInfo(dirs)}")
                      .Append("}");
            }
            return result.ToString();
        }
        #endregion

        #region 写入文本
        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="isAppend">是否追加</param>
        /// <param name="encoding">编码格式</param>
        public static void WriteFile(string content, string filePath, bool isAppend = false, string encoding = "utf-8")
        {
            try
            {
                logWriteLock.EnterWriteLock();
                IsExist(filePath);
                using var sw = new StreamWriter(filePath, isAppend, Encoding.GetEncoding(encoding));
                sw.Write(content);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                logWriteLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="isAppend">是否追加</param>
        /// <param name="encoding">编码格式</param>
        public static async Task WriteFileAsync(string content, string filePath, bool isAppend = false, string encoding = "utf-8")
        {
            try
            {
                logWriteLock.EnterWriteLock();
                IsExist(filePath);
                using var sw = new StreamWriter(filePath, isAppend, Encoding.GetEncoding(encoding));
                await sw.WriteAsync(content);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                logWriteLock.ExitWriteLock();
            }
        }
        #endregion

        #region 检测文件
        /// <summary>
        /// 判断文件是否存在 不存在则创建
        /// </summary>
        /// <param name="path">物理绝对路径</param>
        public static void IsExist(string path)
        {
            if (!path.IsNull() && !File.Exists(path))
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.Create(path).Close();
            }
        }
        #endregion

        #region 复制文件
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sourceFileName">源路径</param>
        /// <param name="destFileName">目的路径</param>
        /// <param name="overwrite">是否覆盖，默认：否</param>
        /// <returns>bool</returns>
        public static bool FileCopy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            var isExist = File.Exists(destFileName);

            if (!overwrite && isExist)
                return true;

            if (overwrite && isExist)
                File.Delete(destFileName);

            using var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs = File.Create(destFileName);
            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = fStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
            return true;
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sourceFileName">源路径</param>
        /// <param name="destFileName">目的路径</param>
        /// <param name="overwrite">是否覆盖，默认：否</param>
        /// <returns>bool</returns>
        public static async Task<bool> FileCopyAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            var isExist = File.Exists(destFileName);

            if (!overwrite && isExist)
                return true;

            if (overwrite && isExist)
                File.Delete(destFileName);

            using var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs = File.Create(destFileName);
            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = await fStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await fs.WriteAsync(buffer, 0, bytesRead);
            }
            return true;
        }
        #endregion

        #region 移动文件
        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourceFileName">源路径</param>
        /// <param name="destFileName">目的路径</param>
        /// <param name="overwrite">是否覆盖，默认：否</param>
        /// <returns>bool</returns>
        public static bool FileMove(string sourceFileName, string destFileName, bool overwrite = false)
        {
            var isExist = File.Exists(destFileName);

            if (!overwrite && isExist)
                return true;

            if (overwrite && isExist)
                File.Delete(destFileName);

            using var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs = File.Create(destFileName);
            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = fStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }

            if (File.Exists(sourceFileName))
                File.Delete(sourceFileName);

            return true;
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourceFileName">源路径</param>
        /// <param name="destFileName">目的路径</param>
        /// <param name="overwrite">是否覆盖，默认：否</param>
        /// <returns>bool</returns>
        public static async Task<bool> FileMoveAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            var isExist = File.Exists(destFileName);

            if (!overwrite && isExist)
                return true;

            if (overwrite && isExist)
                File.Delete(destFileName);

            using var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs = File.Create(destFileName);
            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = await fStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await fs.WriteAsync(buffer, 0, bytesRead);
            }

            if (File.Exists(sourceFileName))
                File.Delete(sourceFileName);

            return true;
        }
        #endregion

        #region 获取文件MD5值
        /// <summary>
        /// 获取文件MD5 hash值
        /// </summary>
        /// <param name="filePath">文件路径(包含文件扩展名)</param>
        /// <returns>string</returns>
        public static string GetMD5HashFromFile(string filePath)
        {
            var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return GetMD5HashFromStream(file);
        }

        /// <summary>
        /// 获取文件流MD5 hash值
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <returns>string</returns>
        public static string GetMD5HashFromStream(Stream fileStream)
        {
            using var md5 = new MD5CryptoServiceProvider();
            using (fileStream)
            {
                var retVal = md5.ComputeHash(fileStream);
                var sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        #endregion

        #region base64数据保存到文件
        /// <summary>
        /// html5 base64数据保存到文件
        /// </summary>
        /// <param name="data">base64数据</param>
        /// <param name="filePath">文件路径(包含文件扩展名)</param>
        /// <returns>bool</returns>
        public static bool SaveBase64ToFile(string data, string filePath)
        {
            var index = data?.ToLower().IndexOf("base64,") ?? -1;
            if (index > -1)
            {
                data = data.Substring(index + 7);
                using var fs = File.Create(filePath);
                var bytes = Convert.FromBase64String(data);
                fs.Write(bytes, 0, bytes.Length);
                return true;
            }
            return false;
        }

        /// <summary>
        /// html5 base64数据保存到文件
        /// </summary>
        /// <param name="data">base64数据</param>
        /// <param name="filePath">文件路径(包含文件扩展名)</param>
        /// <returns>bool</returns>
        public static async Task<bool> SaveBase64ToFileAsync(string data, string filePath)
        {
            var index = data?.ToLower().IndexOf("base64,") ?? -1;
            if (index > -1)
            {
                data = data.Substring(index + 7);
                using var fs = File.Create(filePath);
                var bytes = Convert.FromBase64String(data);
                await fs.WriteAsync(bytes, 0, bytes.Length);
                return true;
            }
            return false;
        }
        #endregion

        #region 获取文件ContentType
        /// <summary>
        /// 获取文件的ContentType类型
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static string GetContentType(this string fileName)
        {
            //文件ContentType驱动
            var provider = new FileExtensionContentTypeProvider();

            //文件后缀名称
            var suffix = Path.GetExtension(fileName);

            //获取文件ContentType
            if (provider.Mappings.ContainsKey(suffix))
                return provider.Mappings[suffix];

            return "application/octet-stream";
        }
        #endregion
    }
}
