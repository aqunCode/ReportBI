namespace Bi.Entities.Entity;

public class SyntaxModel
{
    /// <summary>
    /// 模板编码
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// 模板主体
    /// </summary>
    public List<SyntaxModelItem>? ModelItems { get; set; }
    /// <summary>
    /// 模板的返回值类型  DateTime ， String ，Number
    /// </summary>
    public SyntaxDataType DataType;
    /// <summary>
    /// 字符类型
    /// </summary>
    public ItemType IType { get; set; }
}

public class SyntaxModelItem
{
    /// <summary>
    /// 模型的值，关键字的话填写具体的值，不是关键字的话，为空
    /// </summary>
    public string? Key { get; set; }
    /// <summary>
    /// 字符类型
    /// </summary>
    public ItemType IType { get; set; }
    /// <summary>
    /// 模板的返回值类型  DateTime ， String ，Number
    /// </summary>
    public SyntaxDataType DataType;
    /// <summary>
    /// 是否参与循环   [0 :不循环 1 :循环 2 :是最后一次不循环]
    /// </summary>
    public int Circ { get; set; } = 0;
    /// <summary>
    /// 循环体大小 4,那碰到循环结尾需要减去3 ，默认为 -1 
    /// </summary>
    public int CircNum { get; set; } = -1;
    /// <summary>
    /// 是否可以忽略 0是不忽略  1 是忽略
    /// </summary>
    public int Ignore { get; set; } = 0;
    /// <summary>
    /// 忽略数量 默认0
    /// </summary>
    public int IgnoreNum { get; set; } = 0;

}

public class SyntaxFieldEntity
{
    /// <summary>
    /// 当前字符
    /// </summary>
    public string? Name { set; get; }
    /// <summary>
    /// 当前字符类型
    /// </summary>
    public ItemType Type { set; get; }
}

public enum SyntaxDataType
{
    /// <summary>
    /// 字符串类型
    /// </summary>
    String,
    /// <summary>
    /// 数字类型
    /// </summary>
    Number,
    /// <summary>
    /// 时间类型
    /// </summary>
    DateTime,
    /// <summary>
    /// 默认，任意
    /// </summary>
    Arbitrary
}

public enum ItemType
{
    /// <summary>
    /// 关键字
    /// </summary>
    Keyword,
    /// <summary>
    /// 列名
    /// </summary>
    ColumnName,
    /// <summary>
    /// 运算符
    /// </summary>
    Operators,
    /// <summary>
    /// 括号
    /// </summary>
    Bracket,
    /// <summary>
    /// 字符串
    /// </summary>
    Character,
    /// <summary>
    /// 时间
    /// </summary>
    DateTime,
    /// <summary>
    /// 数字
    /// </summary>
    Num,
    /// <summary>
    /// 错误的类型
    /// </summary>
    Error
}

