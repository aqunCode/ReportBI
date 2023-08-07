
namespace Bi.Entities.Entity;

public class ColumnInfo
{
    /// <summary>
    /// 节点ID
    /// </summary>
    public string? NodeId { get; set; }
    /// <summary>
    /// 数据集ID
    /// </summary>
    public string? DatasetId { get; set; }
    /// <summary>
    /// 表标签名
    /// </summary>
    public string? LabelName { set; get; }
    /// <summary>
    /// 列名
    /// </summary>
    public string? ColumnName { set; get; }
    /// <summary>
    /// 类型
    /// </summary>
    public string? ColumnType { set; get; }
    /// <summary>
    /// 维度、指标分类
    /// </summary>
    public string? DataType { set; get; }
    /// <summary>
    /// 列注释
    /// </summary>
    public string? ColumnComment { set; get; }


}

