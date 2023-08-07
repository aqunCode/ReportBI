using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;

namespace Bi.Services.IService;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：7/4/2023 3:58:57 PM
/// 版本：1.1
/// </summary>

public interface IBiCollectServices: IDependency
{
    /// <summary>
    /// 添加新的BiCollect
    /// </summary>
    Task<double> addAsync(BiCollectInput input);
    /// <summary>
    /// 根据ID删除BiCollect
    /// </summary>
    Task<double> deleteAsync(BiCollectInput input);
    /// <summary>
    /// 获取所有BiCollect
    /// </summary>
    Task<IEnumerable<BiCollect>> getList(BiCollectInput input);
}

