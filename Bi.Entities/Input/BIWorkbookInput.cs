using Bi.Core.Models;
using Bi.Entities.Entity;

namespace Bi.Entities.Input;

public class BIWorkbookInput:BaseInput
{
    public string? Id { get; set; }
    /// <summary>
    /// 工作簿名称
    /// </summary>
    public string? WorkBookName { get; set; }
    /// <summary>
    /// 工作簿code
    /// </summary>
    public string? WorkBookCode { get; set; }
    /// <summary>
    /// 最大显示数量
    /// </summary>
    public int MaxNumber { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string? Des { get; set; }
    /// <summary>
    /// 列名，模糊查询输入条件
    /// </summary>
    public string? ColumnName { get; set; }
    /// <summary>
    /// 数据集code
    /// </summary>
    public string? DatasetId { get; set; }
    /// <summary>
    /// 标记json字段信息
    /// </summary>
    public string? MarkStyle { get; set; }
    /// <summary>
    /// 标记json字段信息
    /// </summary>
    public List<BiMarkField>? MarkItems { get; set; }
    /// <summary>
    /// 过滤的筛选器字段list
    /// </summary>
    public List<BiFilterField>? FilterItems { get; set; }
    /// <summary>
    /// 行列list
    /// </summary>
    public List<BiCalcField>? CalcItems { get; set; }
    /// <summary>
    /// Id
    /// </summary>
    public string? NodeId { get; set; }
    /// <summary>
    /// 厂区
    /// </summary>
    public string? Opt1 { get; set; }
    /// <summary>
    /// 机种
    /// </summary>
    public string? Opt2 { get; set; }
    /// <summary>
    /// 是否公共模板
    /// </summary>
    public string? Opt3 { get; set; }

    public string? Opt4 { get; set; }

}
