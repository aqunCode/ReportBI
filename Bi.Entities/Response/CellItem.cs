using Newtonsoft.Json.Linq;

namespace Bi.Entities.Response;

public class CellItem
{
    /// <summary>
    /// 原始值
    /// </summary>
    public JObject? Original { get; set; }
    /// <summary>
    /// 当前数据类型
    /// </summary>
    public CellType? SingleCellType { get; set; }
    /// <summary>
    /// 是否是最父
    /// </summary>
    public Boolean FinalFather { get; set; } = false;
    /// <summary>
    /// 数据集
    /// </summary>
    public string? SetCode { get; set; }
    /// <summary>
    /// 数据集中的key
    /// </summary>
    public string? SetKey { get; set; }
    /// <summary>
    /// 行
    /// </summary>
    public int Row { get; set; } = -1;
    /// <summary>
    /// 列
    /// </summary>
    public int Column { get; set; } = -1;
    /// <summary>
    /// 行合并
    /// </summary>
    public int RowMerge { get; set; } = -1;
    /// <summary>
    /// 列合并
    /// </summary>
    public int ColumnMerge { get; set; } = -1;
    /// <summary>
    /// 原始行
    /// </summary>
    public int CoordinateRow { get; set; } = -1;
    /// <summary>
    /// 原始列
    /// </summary>
    public int CoordinateColumn { get; set; } = -1;
    /// <summary>
    /// 值
    /// </summary>
    public string? Value { get; set; }
    /// <summary>
    /// 值类型
    /// </summary>
    public string ValueType { get; set; } = "string";
    /// <summary>
    /// 追溯值
    /// </summary>
    public JObject? ReviewValue { get; set; }
    /// <summary>
    /// 设定值
    /// </summary>
    public string? SetValue { get; set; }
    /// <summary>
    /// 拓展方向
    /// </summary>
    public string? Expend { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public string? ExpendSort { get; set; }
    /// <summary>
    /// 左父行
    /// </summary>
    public int LeftParentRow { get; set; } = -1;
    /// <summary>
    /// 左父列
    /// </summary>
    public int LeftParentColumn { get; set; } = -1;
    /// <summary>
    /// 上父行
    /// </summary>
    public int TopParentRow { get; set; } = -1;
    /// <summary>
    /// 上父列
    /// </summary>
    public int TopParentColumn { get; set; } = -1;
    /// <summary>
    /// 数据设置1 分组，列表，汇总
    /// </summary>
    public string ShowType { get; set; } = "list";
    /// <summary>
    /// 数据设置2 普通，相邻连续，高级
    /// </summary>
    public string ShowValue { get; set; }
    /// <summary>
    /// 数据筛选表达式
    /// </summary>
    public string FilterData { get; set; }
    /// <summary>
    /// 将父格子作为筛选条件（适用于父子格来源于一个数据集）
    /// </summary>
    public Boolean IsFather { get; set; }
    /// <summary>
    /// 位于哪个组
    /// </summary>
    public int Group { get; set; }
    /// <summary>
    /// 位于组的第几个元素
    /// </summary>
    public int GroupIndex { get; set; }

    public JObject toJson(JObject original)
    {
        JObject resOriginal = original.DeepClone().ToObject<JObject>();
        resOriginal["r"] = this.Row;
        resOriginal["c"] = this.Column;
        resOriginal["v"]["v"] = this.Value;
        resOriginal["v"]["valueType"] = this.ValueType;
        if (this.RowMerge != -1 || this.ColumnMerge != -1)
        {
            var mc = resOriginal.SelectToken("v.mc");
            if (mc == null)
            {
                mc = new JObject();
            }

            if (this.ColumnMerge != -1)
            {
                mc["cs"] = this.ColumnMerge;
            }
            if (this.RowMerge != -1)
            {
                mc["rs"] = this.RowMerge;
            }
            resOriginal["v"]["mc"] = mc;
        }
        return resOriginal;
    }

    /// <summary>
    /// 构造方法，将cellData 中的每个对象JSON转成对象
    /// </summary>
    /// <param name="item"></param>
    public CellItem(JObject item)
    {
        this.Original = item;
        // 获取单元格类型
        this.SingleCellType = getCellType(item);
        this.Row = Convert.ToInt32(item["r"]);
        this.Column = Convert.ToInt32(item["c"]);
        // 这里直接取值当前的行和列
        this.CoordinateRow = this.Row;
        this.CoordinateColumn = this.Column;
        // 合并单元格
        var v = item["v"].ToObject<JObject>();
        if (v.ContainsKey("mc"))
        {
            this.RowMerge = v.SelectToken("mc.rs") == null ? -1 : Convert.ToInt32(v.SelectToken("mc.rs").ToString());
            this.ColumnMerge = v.SelectToken("mc.cs") == null ? -1 : Convert.ToInt32(v.SelectToken("mc.cs").ToString());
        }
        this.Value = item.SelectToken("v.v")?.ToString();
        if (this.Value != null && this.Value.IndexOf("#{") != -1 && this.Value.IndexOf("}") != -1)
        {
            var arr = this.Value.Replace("#{", "").Replace("}", "").Split('.');
            this.SetCode = arr[0];
            this.SetKey = arr[1];
            // 此处默认只要是动态数据默认是纵向拓展
            this.Expend = "portrait";
        }
        this.SetValue = item.SelectToken("v.m")?.ToString();
        var cellAttribute = item.SelectToken("v.cellAttribute");
        if (cellAttribute != null)
        {
            this.Expend = cellAttribute.SelectToken("expend.expend").ToString();
            this.ExpendSort = cellAttribute.SelectToken("expend.expendSort").ToString();
            var leftParentValue = cellAttribute.SelectToken("expend.leftParentValue");
            if (!String.IsNullOrEmpty(leftParentValue.ToString()))
            {
                var arr = leftParentValue["value"].ToString().Split(',');
                this.LeftParentRow = Convert.ToInt32(arr[0]);
                this.LeftParentColumn = Convert.ToInt32(arr[1]);
            }
            var topParentValue = cellAttribute.SelectToken("expend.topParentValue");
            if (!String.IsNullOrEmpty(topParentValue.ToString()))
            {
                var arr = topParentValue["value"].ToString().Split(',');
                this.TopParentRow = Convert.ToInt32(arr[0]);
                this.TopParentColumn = Convert.ToInt32(arr[1]);
            }
        }
        // 数据显示方式
        var cell = item.SelectToken("v.cell");
        if(cell != null)
        {
            this.ShowType = cell.SelectToken("showType")?.ToString();
            this.ShowValue = cell.SelectToken("showTypeValue")?.ToString();
            this.FilterData = cell.SelectToken("filterData")?.ToString();
            this.IsFather = cell.SelectToken("filterData")?.ToString() == "true";
        }
    }
    /// <summary>
    /// 空参构造
    /// </summary>
    public CellItem() { }
    /// <summary>
    /// 根据 celldata 中记录的单元格信息判断单元格属性
    /// </summary>
    public CellType getCellType(JObject cellObject)
    {
        JObject cellV1 = null;
        if (cellObject["v"].Type == JTokenType.Object)
        {
            cellV1 = cellObject["v"].ToObject<JObject>();
        }
        if (null != cellV1 && cellV1.ContainsKey("v") && !String.IsNullOrEmpty(cellV1["v"].ToString()))
        {
            string cellV2 = cellObject["v"]["v"].ToString();
            JToken mc = cellObject["v"]["mc"];
            if (cellV2.Contains("#{") && cellV2.Contains("}"))
            {
                // 动态单元格
                if (mc != null)
                {
                    return CellType.DYNAMIC_MERGE;
                }
                else
                {
                    return CellType.DYNAMIC;
                }
            }
            else
            {
                //静态单元格
                if (mc != null)
                {
                    return CellType.STATIC_MERGE;
                }
                else
                {
                    return CellType.STATIC;
                }

            }
        }
        else
        {
            return CellType.BLACK;
        }
    }
}
/// <summary>
/// 单元格属性序列
/// </summary>
public enum CellType
{
    /// <summary>
    /// 动态合并单元格
    /// </summary>
    DYNAMIC_MERGE,
    /// <summary>
    /// 动态
    /// </summary>
    DYNAMIC,
    /// <summary>
    /// 静态
    /// </summary>
    STATIC,
    /// <summary>
    /// 静态合并单元格
    /// </summary>
    STATIC_MERGE,
    /// <summary>
    /// 空白
    /// </summary>
    BLACK
}

