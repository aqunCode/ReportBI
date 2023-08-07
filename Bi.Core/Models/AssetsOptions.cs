namespace Bi.Core.Models
{
    /// <summary>
    /// 静态资源配置
    /// </summary>
    public class AssetsOptions
    {
        /// <summary>
        /// 允许的文件类型
        /// </summary>
        public string FileTypes { get; set; }

        /// <summary>
        /// 资源类型: image、video、txt、pdf、doc、excel、ppt等 
        /// </summary>
        public string AssetsType { get; set; }

        /// <summary>
        /// 最大文件大小
        /// </summary>
        public decimal MaxSize { get; set; }

        /// <summary>
        /// 文件目录
        /// </summary>
        public string FilePath { get; set; }
    }
}
