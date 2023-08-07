using Bi.Core.Models;
using MessagePack;

namespace Bi.Entities.Input
{
    [MessagePackObject(true)]
    public class ParameterInput:BaseInput
    {
        /// <summary>
        /// --数据集Code
        /// </summary>
        public string DATASETCODE { get; set; }
        /// <summary>
        /// --参数名称
        /// </summary>
        public string PARAMETERNAME { get; set; }

        /// <summary>
        /// --参数类型
        /// </summary>
        public string PARAMETERTYPE { get; set; }
        /// <summary>
        /// --参数值
        /// </summary>
        public string PARAMETERVALUE { get; set; }
        /// <summary>
        /// --是否启用(0:不启用，1：启用)
        /// </summary>
        public int ENABLED { get; set; }                                  
            
        /// <summary>
        /// --扩展1
        /// </summary>
        /// <returns></returns>
        public string OPT1 { get; set; }

        /// <summary>
        /// --扩展2
        /// </summary>
        public string OPT2 { get; set; }

        /// <summary>
        ///  --扩展3
        /// </summary>
        public string OPT3 { get; set; }     
        
       /// <summary>
       /// --扩展4
       /// </summary>
        public string OPT4 { get; set; }
        /// <summary>
        /// --扩展5
        /// </summary>
        public string OPT5 { get; set; }                           
    }
}
