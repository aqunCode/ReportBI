using Bi.Core.Interfaces;
using Bi.Entities.Entity;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IIndexService : IDependency
{
    /// <summary>
    /// 获取首页抬头用户访问记录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<BiRecord> getRecord(IndexInput input);

    /// <summary>
    /// 获取用户访问top5
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<List<BiFrequency>> getTopFive(IndexInput input);
    /// <summary>
    /// 单机top5 折线图
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<List<BiChartRecord>> getTopChartRecord(IndexInput input);
    /// <summary>
    /// 获取模型占比
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<List<BiModelRecord>> getModelRecord(IndexInput input);
}
