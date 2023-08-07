using Bi.Core.Models;
using MessagePack;
using SqlSugar;

namespace Bi.Entities.Entity;

[SugarTable("BI_WORKBOOK")]
[MessagePackObject(true)]
public class BIWorkbook : BaseEntity
{
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
    /// 数据集ID
    /// </summary>
    public string? DatasetId { get; set; }
    /// <summary>
    /// 标记json字段信息
    /// </summary>
    public string? MarkStyle { get; set; }
    /// <summary>
    /// 标记json字段信息
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<BiMarkField>? MarkItems { get; set; }
    /// <summary>
    /// 过滤的筛选器字段list
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<BiFilterField>? FilterItems { get; set; }
    /// <summary>
    /// 行列list
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<BiCalcField>? CalcItems { get; set; }
    /// <summary>
    /// 保存的时候生成的动态sql，不包含筛选器
    /// </summary>
    public string? DynamicSql { get; set; }
    /// <summary>
    /// 多余备选字段
    /// </summary>
    public string? Opt1 { get; set; }
    
    public string? Opt2 { get; set; }
   
    public string? Opt3 { get; set; }
    
    public string? Opt4 { get; set; }
    /// <summary>
    /// 是否已经删除
    /// </summary>
    public string DeleteFlag { get; set; }

    public BIWorkbook() => DeleteFlag = "N";

}
