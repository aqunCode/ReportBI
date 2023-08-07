using System;
using System.IO;
using System.IO.Compression;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// 解压缩工具类
    /// </summary>
    public class CompressHelper
    {
        #region gzip
        /// <summary>
        /// gzip压缩字节数组
        /// </summary>
        /// <param name="arr">源字节数组</param>
        /// <returns>压缩后的字节数组</returns>
        public static byte[] GZipCompress(byte[] arr)
        {
            using (var stream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    gZipStream.Write(arr, 0, arr.Length);
                }
                return stream.ToArray();
            }
        }

        /// <summary>
        /// gzip解压字节数组
        /// </summary>
        /// <param name="arr">源字节数组</param>
        /// <returns>解压缩后的字节数组</returns>
        public static byte[] GZipDecompress(byte[] arr)
        {
            using (var ms = new MemoryStream())
            {
                using (var stream = new MemoryStream(arr))
                {
                    using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(ms, 10240);
                        return ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// gzip压缩字节流
        /// </summary>
        /// <param name="sourceStream">源字节流</param>
        /// <returns>压缩后的字节流</returns>
        public static Stream GZipCompress(Stream sourceStream)
        {
            using (sourceStream)
            {
                var bytesArr = new byte[sourceStream.Length];
                sourceStream.Read(bytesArr, 0, bytesArr.Length);
                var gZipArr = GZipCompress(bytesArr);
                return new MemoryStream(gZipArr);
            }
        }

        /// <summary>
        /// gzip解压字节流
        /// </summary>
        /// <param name="sourceStream">源字节流</param>
        /// <returns>解压缩后的字节流</returns>
        public static Stream GZipDecompress(Stream sourceStream)
        {
            using (sourceStream)
            {
                var bytesArr = new byte[sourceStream.Length];
                sourceStream.Read(bytesArr, 0, bytesArr.Length);
                var gZipArr = GZipDecompress(bytesArr);
                return new MemoryStream(gZipArr);
            }
        }
        #endregion

        #region deflate
        /// <summary>
        /// deflate压缩字节数组
        /// </summary>
        /// <param name="arr">源字节数组</param>
        /// <returns>压缩后的字节数组</returns>
        public static byte[] DeflateCompress(byte[] arr)
        {
            using (var stream = new MemoryStream())
            {
                using (var gZipStream = new DeflateStream(stream, CompressionMode.Compress))
                {
                    gZipStream.Write(arr, 0, arr.Length);
                }
                return stream.ToArray();
            }
        }

        /// <summary>
        /// deflate解压字节数组
        /// </summary>
        /// <param name="arr">源字节数组</param>
        /// <returns>解压缩后的字节数组</returns>
        public static byte[] DeflateDecompress(byte[] arr)
        {
            using (var ms = new MemoryStream())
            {
                using (var stream = new MemoryStream(arr))
                {
                    using (var gZipStream = new DeflateStream(stream, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(ms, 10240);
                        return ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// deflate压缩字节流
        /// </summary>
        /// <param name="sourceStream">源字节流</param>
        /// <returns>压缩后的字节流</returns>
        public static Stream DeflateCompress(Stream sourceStream)
        {
            using (sourceStream)
            {
                var bytesArr = new byte[sourceStream.Length];
                sourceStream.Read(bytesArr, 0, bytesArr.Length);
                var gZipArr = DeflateCompress(bytesArr);
                return new MemoryStream(gZipArr);
            }
        }

        /// <summary>
        /// deflate解压字节流
        /// </summary>
        /// <param name="sourceStream">源字节流</param>
        /// <returns>解压缩后的字节流</returns>
        public static Stream DeflateDecompress(Stream sourceStream)
        {
            using (sourceStream)
            {
                var bytesArr = new byte[sourceStream.Length];
                sourceStream.Read(bytesArr, 0, bytesArr.Length);
                var gZipArr = DeflateDecompress(bytesArr);
                return new MemoryStream(gZipArr);
            }
        }
        #endregion
    }
}
