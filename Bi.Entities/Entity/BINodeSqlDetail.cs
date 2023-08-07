using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity
{
    [SugarTable("BI_DATASET_NODE_SQLDETAIL")]
    public class BINodeSqlDetail : BaseEntity
    {
        /// <summary>
        /// --数据集CodeId
        /// </summary>
        public string? DataSetCode { get; set; }
        /// <summary>
        /// --数据集节点CodeId
        /// </summary>
        public string? DataSetNodeId { get; set; }
        /// <summary>
        /// --自定义SQL
        /// </summary>
        public string? CustomizationSql { get; set; }
        /// <summary>
        /// --别名
        /// </summary>
        public string? Alias { get; set; }
        /// <summary>
        /// --参数条件
        /// </summary>
        public string? Parameter { get; set; }
        /// <summary>
        /// --扩展1
        /// </summary>
        public string? Opt1 {get;set;}
        /// <summary>
        /// --扩展2
        /// </summary>
        public string? Opt2 { get; set; }
        /// <summary>
        ///  --扩展3
        /// </summary>
        public string? Opt3 { get; set; }
        /// <summary>
        ///  --扩展4
        /// </summary>
        public string? Opt4 { get; set; }
        /// <summary>
        ///  --扩展5
        /// </summary>
        public string? Opt5 { get; set; }                
    }
}
