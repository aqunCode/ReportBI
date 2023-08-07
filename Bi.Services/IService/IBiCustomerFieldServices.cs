
using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using System.Collections.Generic;
namespace Bi.Services.IService;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2022/12/28 16:54:37
/// 版本：1.0
/// </summary>

public interface IBiCustomerFieldServices: IDependency
{
    /// <summary>
    /// 添加新的BiCustomerField
    /// </summary>
    Task<double> addAsync(BiCustomerFieldInput input);
    /// <summary>
    /// 根据ID删除BiCustomerField
    /// </summary>
    Task<double> deleteAsync(BiCustomerFieldInput input);
    /// <summary>
    /// 更新BiCustomerField
    /// </summary>
    Task<double> ModifyAsync(BiCustomerFieldInput input);
    /// <summary>
    /// 分页获取所有BiCustomerField
    /// </summary>
    Task<PageEntity<IEnumerable<BiCustomerField>>> getPagelist(PageEntity<BiCustomerFieldInput> inputs);
    /// <summary>
    /// 获取单个实体
    /// </summary>
    Task<(double,BiCustomerField)> getEntity(BiCustomerFieldInput input);
    /// <summary>
    /// 语法检查
    /// </summary>
    Task<(string,string)> syntaxRules(BiCustomerFieldInput input);
}

