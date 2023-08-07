﻿using Bi.Core.Extensions;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// Zip解压缩帮助类
    /// </summary>
    public class ZipHelper
    {
        #region Zip压缩
        /// <summary>   
        /// 递归压缩文件夹的内部方法   
        /// </summary>   
        /// <param name="folderToZip">要压缩的文件夹路径</param>   
        /// <param name="zipStream">压缩输出流</param>   
        /// <param name="parentFolderName">此文件夹的上级文件夹</param>   
        /// <returns></returns>   
        private static bool ZipDirectory(string folderToZip, ZipOutputStream zipStream, string parentFolderName)
        {
            string[] folders, files;
            var crc = new Crc32();
            var ent = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/")) { IsUnicodeText = true };
            zipStream.PutNextEntry(ent);
            zipStream.Flush();
            files = Directory.GetFiles(folderToZip);
            foreach (var file in files)
            {
                using var fs = File.OpenRead(file);
                var buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                ent = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/" + Path.GetFileName(file))) { IsUnicodeText = true };
                ent.DateTime = DateTime.Now;
                ent.Size = fs.Length;
                crc.Reset();
                crc.Update(buffer);
                ent.Crc = crc.Value;
                zipStream.PutNextEntry(ent);
                zipStream.Write(buffer, 0, buffer.Length);
            }
            folders = Directory.GetDirectories(folderToZip);
            foreach (var folder in folders)
            {
                if (!ZipDirectory(folder, zipStream, folderToZip))
                    return false;
            }
            return true;
        }

        /// <summary>   
        /// Zip压缩文件夹    
        /// </summary>   
        /// <param name="folderToZip">要压缩的文件夹路径</param>   
        /// <param name="zipedFile">压缩文件完整路径</param>   
        /// <param name="password">密码，默认：null</param>   
        /// <returns>是否压缩成功</returns>   
        public static bool ZipDirectory(string folderToZip, string zipedFile, string password = null)
        {
            if (!Directory.Exists(folderToZip))
                return false;

            using var zipStream = new ZipOutputStream(File.Create(zipedFile));
            zipStream.SetLevel(6);

            if (password.IsNotNullOrEmpty())
                zipStream.Password = password;

            return ZipDirectory(folderToZip, zipStream, "");
        }

        /// <summary>
        /// 压缩文件的内部方法
        /// </summary>
        /// <param name="fileToZip">要压缩的文件全名</param>
        /// <param name="zipStream">压缩输出流</param>
        /// <param name="password">密码，默认：null</param>
        /// <returns>压缩结果</returns>
        private static bool ZipFile(string fileToZip, ZipOutputStream zipStream, string password = null)
        {
            if (!File.Exists(fileToZip))
                return false;

            using var fs = File.OpenRead(fileToZip);
            var buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);

            if (password.IsNotNullOrEmpty())
                zipStream.Password = password;

            var ent = new ZipEntry(Path.GetFileName(fileToZip)) { IsUnicodeText = true };
            zipStream.PutNextEntry(ent);
            zipStream.SetLevel(6);
            zipStream.Write(buffer, 0, buffer.Length);
            return true;
        }

        /// <summary>   
        /// Zip压缩文件   
        /// </summary>   
        /// <param name="fileToZip">要压缩的文件全名</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码，默认：null</param>   
        /// <returns>压缩结果</returns>   
        public static bool ZipFile(string fileToZip, string zipedFile, string password = null)
        {
            if (!File.Exists(fileToZip))
                return false;

            using var fs = File.OpenRead(fileToZip);
            var buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            using var f = File.Create(zipedFile);
            using var zipStream = new ZipOutputStream(f);

            if (password.IsNotNullOrEmpty())
                zipStream.Password = password;

            var ent = new ZipEntry(Path.GetFileName(fileToZip)) { IsUnicodeText = true };
            zipStream.PutNextEntry(ent);
            zipStream.SetLevel(6);
            zipStream.Write(buffer, 0, buffer.Length);
            return true;
        }

        /// <summary>   
        /// Zip压缩文件或文件夹   
        /// </summary>   
        /// <param name="fileToZip">要压缩的路径</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码，默认：null</param>   
        /// <returns>压缩结果</returns>   
        public static bool Zip(string fileToZip, string zipedFile, string password = null)
        {
            var result = false;

            if (Directory.Exists(fileToZip))
                result = ZipDirectory(fileToZip, zipedFile, password);
            else if (File.Exists(fileToZip))
                result = ZipFile(fileToZip, zipedFile, password);

            return result;
        }

        /// <summary>
        /// Zip压缩文件或文件夹
        /// </summary>
        /// <param name="filesToZip">要批量压缩的路径或者文件夹</param>
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <param name="password">密码，默认：null</param>
        /// <returns>压缩结果</returns>
        public static bool Zip(List<string> filesToZip, string zipedFile, string password = null)
        {
            var result = true;
            using var zipStream = new ZipOutputStream(File.Create(zipedFile));
            zipStream.SetLevel(6);

            if (password.IsNotNullOrEmpty())
                zipStream.Password = password;

            filesToZip.ForEach(o =>
            {
                if (Directory.Exists(o))
                {
                    if (!ZipDirectory(o, zipStream, ""))
                        result = false;
                }
                else if (File.Exists(o))
                {
                    if (!ZipFile(o, zipStream, password))
                        result = false;
                }
            });

            return result;
        }
        #endregion

        #region Zip解压
        /// <summary>   
        /// Zip解压功能(解压压缩文件到指定目录)   
        /// </summary>   
        /// <param name="fileToUnZip">待解压的文件</param>   
        /// <param name="zipedFolder">指定解压目标目录</param>   
        /// <param name="password">密码，默认：null</param>   
        /// <returns>解压结果</returns>   
        public static bool UnZip(string fileToUnZip, string zipedFolder, string password = null)
        {
            if (!File.Exists(fileToUnZip))
                return false;

            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);

            using var zipStream = new ZipInputStream(File.OpenRead(fileToUnZip));

            if (password.IsNotNullOrEmpty())
                zipStream.Password = password;

            ZipEntry ent;
            while ((ent = zipStream.GetNextEntry()) != null)
            {
                if (ent.Name.IsNotNullOrEmpty())
                {
                    var fileName = Path.Combine(zipedFolder, ent.Name);
                    fileName = PathHelper.ConvertToCurrentOsPath(fileName);
                    if (fileName.EndsWith(PathHelper.CurrentOsDirectorySeparator.ToString()))
                    {
                        Directory.CreateDirectory(fileName);
                        continue;
                    }
                    using var fs = File.Create(fileName);
                    var buffer = new byte[2048];
                    var bytesRead = 0;
                    //每次读取2kb数据，然后写入文件
                    while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                    }
                }
            }
            return true;
        }
        #endregion

        #region BZip2压缩
        /// <summary>
        /// BZip2压缩
        /// </summary>
        /// <param name="data">源字节数组</param>
        /// <returns></returns>
        public static byte[] BZip2(byte[] data)
        {
            using var mstream = new MemoryStream();

            using var zipOutStream = new BZip2OutputStream(mstream);
            zipOutStream.Write(data, 0, data.Length);
            byte[] result = mstream.ToArray();

            return result;
        }

        /// <summary>
        /// BZip2压缩
        /// </summary>
        /// <param name="data">源字节数组</param>
        /// <param name="level">压缩率，1-9，数字越大压缩率越高</param>
        /// <returns></returns>
        public static byte[] BZip2(byte[] data, int level)
        {
            using var mstream = new MemoryStream();

            using var zipOutStream = new BZip2OutputStream(mstream, level);
            zipOutStream.Write(data, 0, data.Length);
            byte[] result = mstream.ToArray();

            return result;
        }

        /// <summary>
        /// BZip2压缩
        /// </summary>
        /// <param name="zipFileName">压缩后的文件路径</param>
        /// <param name="sourceFileName">要压缩的文件路径</param>
        /// <returns></returns>
        public static bool BZip2(string zipFileName, string sourceFileName)
        {
            if (!File.Exists(sourceFileName))
                return false;

            using var fr = File.OpenRead(sourceFileName);
            var buffer = new byte[fr.Length];
            fr.Read(buffer, 0, buffer.Length);

            using var fc = File.Create(zipFileName);
            using var zipStream = new BZip2OutputStream(fc);
            zipStream.Write(buffer, 0, buffer.Length);

            return true;
        }

        /// <summary>
        /// BZip2压缩
        /// </summary>
        /// <param name="zipFileName">压缩后的文件路径</param>
        /// <param name="sourceFileName">要压缩的文件路径</param>
        /// <param name="level">压缩率，1-9，数字越大压缩率越高</param>
        /// <returns></returns>
        public static bool BZip2(string zipFileName, string sourceFileName, int level)
        {
            if (!File.Exists(sourceFileName))
                return false;

            using var fr = File.OpenRead(sourceFileName);
            var buffer = new byte[fr.Length];
            fr.Read(buffer, 0, buffer.Length);

            using var fc = File.Create(zipFileName);
            using var zipStream = new BZip2OutputStream(fc, level);
            zipStream.Write(buffer, 0, buffer.Length);

            return true;
        }
        #endregion

        #region BZip2解压缩
        /// <summary>
        /// BZip2解压缩
        /// </summary>
        /// <param name="data">要解压的字节数组数据</param>
        /// <returns></returns>
        public static byte[] UnBZip2(byte[] data)
        {
            using var mstream = new MemoryStream(data);

            using var zipInputStream = new BZip2InputStream(mstream);
            using var readstream = new StreamReader(zipInputStream, Encoding.UTF8);
            var unzipdata = readstream.ReadToEnd();

            return Encoding.UTF8.GetBytes(unzipdata);
        }

        /// <summary>
        /// BZip2解压缩
        /// </summary>
        /// <param name="sourceFileName">要解压的zip文件路径</param>
        /// <param name="destFileName">解压后的文件路径</param>
        /// <returns></returns>
        public static bool UnBZip2(string sourceFileName, string destFileName)
        {
            if (!File.Exists(sourceFileName))
                return false;

            using var fr = File.OpenRead(sourceFileName);
            using var zipStream = new BZip2InputStream(fr);
            using var fc = File.Create(destFileName);

            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                fc.Write(buffer, 0, bytesRead);
            }

            return true;
        }
        #endregion

        #region FastZip
        /// <summary>
        /// FastZip
        /// </summary>
        public static FastZip FastZip() => new();

        /// <summary>
        /// FastZip
        /// </summary>
        /// <param name="password"></param>
        /// <param name="compressionLevel"></param>
        /// <param name="createEmptyDirectories"></param>
        /// <param name="useZip64"></param>
        /// <param name="restoreDateTimeOnExtract"></param>
        /// <param name="restoreAttributesOnExtract"></param>
        /// <param name="entryEncryptionMethod"></param>
        /// <returns></returns>
        public static FastZip FastZip(
            string password,
            Deflater.CompressionLevel compressionLevel = Deflater.CompressionLevel.NO_COMPRESSION,
            bool createEmptyDirectories = false,
            UseZip64 useZip64 = UseZip64.Off,
            bool restoreDateTimeOnExtract = false,
            bool restoreAttributesOnExtract = false,
            ZipEncryptionMethod entryEncryptionMethod = ZipEncryptionMethod.ZipCrypto)
        => new()
        {
            Password = password,
            CompressionLevel = compressionLevel,
            CreateEmptyDirectories = createEmptyDirectories,
            UseZip64 = useZip64,
            RestoreDateTimeOnExtract = restoreDateTimeOnExtract,
            RestoreAttributesOnExtract = restoreAttributesOnExtract,
            EntryEncryptionMethod = entryEncryptionMethod
        };
        #endregion

        #region TarGz压缩
        /// <summary>
        /// 创建tar.gz压缩文件，如：ZipHelper.CreateTarGz(@"c:\temp\gzip-test.tar.gz", @"c:\data")
        /// </summary>
        /// <param name="tgzFilename">.tar.gz后缀名待压缩文件名</param>
        /// <param name="sourceDirectory">要压缩的目录</param>
        public static void CreateTarGz(string tgzFilename, string sourceDirectory)
        {
            using var outStream = File.Create(tgzFilename);
            using var gzoStream = new GZipOutputStream(outStream);
            using var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);
        }

        /// <summary>
        /// 递归压缩目录ToTar
        /// </summary>
        /// <param name="tarArchive"></param>
        /// <param name="sourceDirectory"></param>
        /// <param name="recurse"></param>
        private static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            var tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            // Write each file to the tar.
            var filenames = Directory.GetFiles(sourceDirectory);
            foreach (var filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                var directories = Directory.GetDirectories(sourceDirectory);
                foreach (var directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }
        #endregion

        #region TarGz解压缩
        /// <summary>
        /// tar.gz文件解压缩，如：ZipHelper.ExtractTarGz(@"c:\temp\test.tar.gz", @"C:\DestinationFolder")
        /// </summary>
        /// <param name="gzArchiveName">.tar.gz后缀名待解压缩文件名</param>
        /// <param name="destFolder">解压缩的目录</param>
        /// <param name="nameEncoding">解压编码</param>
        public static void ExtractTarGz(string gzArchiveName, string destFolder, string nameEncoding = "utf-8")
        {
            using var inStream = File.OpenRead(gzArchiveName);
            using var gzipStream = new GZipInputStream(inStream);

            using var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.GetEncoding(nameEncoding));
            tarArchive.ExtractContents(destFolder);
        }
        #endregion

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] BytesCompass(byte[] bytes, string pwd = "")
        {
            MemoryStream memStreamIn = new MemoryStream(bytes);
            MemoryStream outputMemStream = new MemoryStream();
            using (ZipOutputStream zipStream = new ZipOutputStream(outputMemStream))
            {

                if (pwd != null)
                {
                    zipStream.Password = pwd;
                }
                zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                ZipEntry newEntry = new ZipEntry("tmp.zip");
                newEntry.DateTime = DateTime.Now;

                zipStream.PutNextEntry(newEntry);

                StreamUtils.Copy(memStreamIn, zipStream, new byte[4096]);
                zipStream.CloseEntry();

                zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
                zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.

                outputMemStream.Position = 0;
                return outputMemStream.ToArray();
            }

        }
        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] BytesDeCompass(byte[] bytes, string pwd = "")
        {
            MemoryStream ms = new MemoryStream(bytes);


            ZipInputStream zipInputStream = new ZipInputStream(ms);
            if (pwd != null)
            {
                zipInputStream.Password = pwd;
            }
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            if (zipEntry != null)
            {

                String entryFileName = zipEntry.Name;

                byte[] buffer = new byte[4096];     // 4K is optimum
                using (MemoryStream streamWriter = new MemoryStream())
                {
                    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                    return streamWriter.ToArray();
                }
            }
            return null;
        }
        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringCompass(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] zipbs = BytesCompass(bytes, "");
            return (string)(Convert.ToBase64String(zipbs));
        }
        /// <summary>
        /// 解压字符串
        /// </summary>
        /// <param name="zipstr"></param>
        /// <returns></returns>
        public static string StringDeCompass(string zipstr)
        {
            byte[] zipbs = BytesDeCompass(Convert.FromBase64String(zipstr), "");
            return Encoding.UTF8.GetString(zipbs);
        }
    }
}
