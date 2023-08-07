using Bi.Core.Models;

namespace Bi.Entities.Input;

public class DataSourceInput :BaseInput {
    /// <summary>
    /// 数据源编码
    /// </summary>
    public string? SourceCode { get; set; }
    /// <summary>
    /// 数据源名称
    /// </summary>
    public string? SourceName { get; set; }
    /// <summary>
    /// 数据源描述
    /// </summary>
    public string? SourceDesc { get; set; }
    /// <summary>
    /// 数据源类型
    /// </summary>
    public string? SourceType { get; set; }
    /// <summary>
    /// DB连接串  数据库
    /// </summary>
    public string? SourceConnect { get; set; }
    /// <summary>
    /// http 请求路径
    /// </summary>
    public string? HttpAddress { get; set; }
    /// <summary>
    /// http 请求方式
    /// </summary>
    public string? HttpWay { get; set; }
    /// <summary>
    /// http 请求头
    /// </summary>
    public string? HttpHeader { get; set; }
    /// <summary>
    /// http 请求体
    /// </summary>
    public string? HttpBody { get; set; }

}

public class DataSourceDelete :BaseInput {
    
    /// <summary>
    /// 数据源编码List
    /// </summary>
    public List<String> sourceCode { get; set; }
}


