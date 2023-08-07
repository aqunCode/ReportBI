using Microsoft.AspNetCore.Http;
using System;

namespace Bi.Core.EasyFS
{
    /// <summary>
    /// 上传参数
    /// </summary>
    public class AppFileInput
    {
        /// <summary>
        /// 上传操作-文件流
        /// </summary>
        public IFormFile FileData { get; set; }

        /// <summary>
        /// 自定义文件Id，若存在则上传失败
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// 自定义文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 上传操作-文件Index,从1开始,默认1
        /// </summary>
        public int? FileIndex { get; set; } = 1;

        /// <summary>
        /// 上传操作-文件Total,默认1
        /// </summary>
        public int? FileTotal { get; set; } = 1;

        /// <summary>
        /// 存储位置,可为空,默认时间。例如:Assests/HeadIcon
        /// </summary>
        public string Directory { get; set; } = DateTime.Now.ToString("yyyyMMdd");
    }
}
