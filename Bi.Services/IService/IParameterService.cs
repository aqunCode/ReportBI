using Bi.Core.Interfaces;
using Bi.Entities.Input;

namespace Bi.Services.IService;

/// <summary>
/// 自定义参数服务接口
/// </summary>
public interface IParameterService: IDependency
{
    /// <summary>
    /// 添加新的Parameter
    /// </summary>
    Task<double> addAsync(ParameterInput input);
    /// <summary>
    /// 根据ID删除Parameter
    /// </summary>
    Task<double> deleteAsync(ParameterInput input);
}