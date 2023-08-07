using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IBIRelationServices : IDependency
{
    /// <summary>
    /// 添加数据集关系
    /// </summary>
    Task<double> addAsync(BIRelationInput input);
    /// <summary>
    /// 删除关系映射
    /// </summary>
    Task<double> deleteAsync(BIRelationInput input);
    /// <summary>
    /// 修改关系映射
    /// </summary>
    Task<double> ModifyAsync(BIRelationInput input);
    /// <summary>
    /// 根据查询
    /// </summary>
    Task<PageEntity<IEnumerable<BIRelation>>> getPagelist(PageEntity<BIRelationInput> inputs);
}
