using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Helpers
{
    public class StringHelper
    {
        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="unCompressedString">要压缩的字符串</param>
        /// <returns></returns>
        public static string Zip(string unCompressedString)
        {
            byte[] bytData = System.Text.Encoding.UTF8.GetBytes(unCompressedString);
            MemoryStream ms = new MemoryStream();
            Stream s = new GZipStream(ms, CompressionMode.Compress);
            s.Write(bytData, 0, bytData.Length);
            s.Close();
            byte[] compressedData = (byte[])ms.ToArray();
            return System.Convert.ToBase64String(compressedData, 0, compressedData.Length);
        }
        /// <summary>
        ///  解压字符串
        /// </summary>
        /// <param name="unCompressedString">要解压的字符串</param>
        /// <returns></returns>
        public static string UnZip(string unCompressedString)
        {
            try
            {
                StringBuilder uncompressedString = new System.Text.StringBuilder();
                byte[] writeData = new byte[4096];
                byte[] bytData = Convert.FromBase64String(unCompressedString);
                int totalLength = 0;
                Stream s = new GZipStream(new MemoryStream(bytData), CompressionMode.Decompress);
                while (true)
                {
                    int size = s.Read(writeData, 0, writeData.Length);
                    if (size > 0)
                    {
                        totalLength += size;
                        uncompressedString.Append(System.Text.Encoding.UTF8.GetString(writeData, 0, size));
                    }
                    else
                    {
                        break;
                    }
                }
                s.Close();
                return uncompressedString.ToString();
            }
            catch (Exception)
            {
                return unCompressedString;
            }
        }
    }
}
