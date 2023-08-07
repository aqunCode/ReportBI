using Bi.Core.Models;
using SqlSugar;

namespace Bi.Entities.Entity;
/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/6/12 15:30:50
/// 版本：1.1
/// </summary>
[SugarTable("BI_CANVAS_IMAGE")]
public class Base64ImageEntity : BaseEntity
{
    public string? ImageJson { get; set; }

    public string? Id { get; set; }
}
