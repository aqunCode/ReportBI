using Bi.Core.Models;
using Bi.Entities.Entity;
using MessagePack;

namespace Bi.Entities.Input;

[MessagePackObject(true)]
public class DataCollectInput : BaseInput
{
    /// <summary>
    /// 数据集下拉框字段模糊查询
    /// </summary>
    public string? Value { set; get; }
    /// <summary>
    /// 返回结果集
    /// </summary>
    public string? CaseResult { set;get;}
    /// <summary>
    /// 查询条件列表
    /// </summary>
    public List<DataCollectItem>? DataSetParamDtoList {set;get;}
    /// <summary>
    /// 数据集类型
    /// </summary>
    public string? SetType { set;get;}
    /// <summary>
    /// 数据源编码
    /// </summary>
    public string? SourceCode { set;get;}
    /// <summary>
    /// 数据转换
    /// </summary>
    public List<string>? DataSetTransformDtoList { set;get; }
    /// <summary>
    /// 数据集编码
    /// </summary>
    public string? SetCode { set;get; }
    /// <summary>
    /// 数据集名称
    /// </summary>
    public string? SetName { set;get; }
    /// <summary>
    /// 数据集描述
    /// </summary>
    public string? SetDesc { set;get; }
    /// <summary>
    /// 动态 sql/动态 请求体
    /// </summary>
    public string? DynSentence { set;get; }
    /// <summary>
    /// 从第几行开始查询(当前仅限数据库)
    /// </summary>
    public int LimitStart { set; get; }
    /// <summary>
    /// 查询到第几行(当前仅限数据库)
    /// </summary>
    public int LimitEnd { set; get; }
    /// <summary>
    /// 排序字段（当前字段会修改sql语句）
    /// </summary>
    public List<string>? OrderArr { set; get; }
    /// <summary>
    /// 最父格筛选值表达式
    /// </summary>
    public string? SqlFiltration { set; get; }
    /// <summary>
    /// 分组信息
    /// </summary>
    public List<string> GroupList { set; get; } = new List<string>();
    /// <summary>
    /// 是否查询全部（当前字段 会修改sql语句）
    /// </summary>
    public Boolean SearchAll { set; get; } = false;
    /// <summary>
    /// 是否导出
    /// </summary>
    public Boolean Export { set; get; } = false;
    /// <summary>
    /// 是否预览
    /// </summary>
    public Boolean IsPreview { set; get; } = false;

}


public class DataCollectReader : BaseInput
{
    /// <summary>
    /// 数据集编码
    /// </summary>
    public string? SetCode { set; get; }
    /// <summary>
    /// 查询条件列表
    /// </summary>
    public List<DataCollectItem>? DataSetParamDtoList { set; get; }
    /// <summary>
    /// 数据集类型
    /// </summary>
    public string? SetType { set; get; }
}

public class DataCollectCount : BaseInput
{
    /// <summary>
    /// 数据集编码
    /// </summary>
    public string? SetCode { set; get; }
    /// <summary>
    /// 查询条件列表
    /// </summary>
    public List<DataCollectItem>? DataSetParamDtoList { set; get; }
    /// <summary>
    /// 数据集类型
    /// </summary>
    public string? SetType { set; get; }
}

public class DataReaderDBTest
{
    /// <summary>
    /// 数据源编码
    /// </summary>
    public string? SourceCode { get; set; }
    /// <summary>
    /// 数据源类型
    /// </summary>
    public string? SourceType { set; get; }
    /// <summary>
    /// 数据源连接串
    /// </summary>
    public string? SourceConnect { set; get; }
    /// <summary>
    /// 所要执行的sql
    /// </summary>
    public string? DynSql { set; get; }
}

public class DataCountDBTest
{
    /// <summary>
    /// 数据源编码
    /// </summary>
    public string? SourceCode { get; set; }
    /// <summary>
    /// 数据源类型
    /// </summary>
    public string? SourceType { set; get; }
    /// <summary>
    /// 数据源连接串
    /// </summary>
    public string? SourceConnect { set; get; }
    /// <summary>
    /// 所要执行的sql
    /// </summary>
    public string? DynSql { set; get; }
}

public class DataCollectDBTest {

    /// <summary>
    /// 数据源编码
    /// </summary>
    public string? SourceCode { get; set; }
    /// <summary>
    /// 数据源类型
    /// </summary>
    public string? SourceType { set; get; }
    /// <summary>
    /// 数据源连接串
    /// </summary>
    public string? SourceConnect { set; get; }
    /// <summary>
    /// 所要执行的sql
    /// </summary>
    public string? DynSql { set; get; }
    /// <summary>
    /// 从第几行开始查询(当前仅限数据库)
    /// </summary>
    public int LimitStart { set; get; }
    /// <summary>
    /// 查询到第几行(当前仅限数据库)
    /// </summary>
    public int LimitEnd { set; get; }
    /// <summary>
    /// 排序字段(当前仅限数据库)
    /// </summary>
    public List<string>? OrderArr { set; get; }
    /// <summary>
    /// 分组信息
    /// </summary>
    public List<string>? GroupList { set; get; }
    /// <summary>
    /// 最父格筛选sql
    /// </summary>
    public string? SqlFiltration { set; get; }
    /// <summary>
    /// 是否查询所有的数据(当前仅限数据库)
    /// </summary>
    public Boolean SearchAll { set; get; } = false;
    /// <summary>
    /// 是否导出
    /// </summary>
    public Boolean Export { set; get; } = false;

}


public class DataCollectDelete : BaseInput {
    /// <summary>
    /// 批量删除 根据setCode
    /// </summary>
    public string[]? SetCode { set; get; }
}

