using Bi.Core.Helpers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Models
{
    /// <summary>
    /// Excel导入类 
    /// 
    /// 继承此类可以使用特性校验
    /// </summary>
    [MessagePackObject(true)]
    public class ExcelInput
    {
        /// <summary>
        /// 序号
        /// </summary>
        [ExcelColumn("序号")]
        public string Number { get; set; }
    }
}
