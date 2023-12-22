using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bi.Services.IService;

/// <summary>
/// 描述： This is the class description
/// 作者：GPF
/// 创建日期：2023/12/14 10:38:03
/// 版本：1.1
/// </summary>

public interface IBiArticleServices: IDependency
{
    /// <summary>
    /// 添加新的BiArticle
    /// </summary>
    Task<double> addAsync(BiArticleInput input);
    /// <summary>
    /// 根据ID删除BiArticle
    /// </summary>
    Task<double> deleteAsync(BiArticleInput input);
    /// <summary>
    /// 更新BiArticle
    /// </summary>
    Task<double> ModifyAsync(BiArticleInput input);
    /// <summary>
    /// 分页获取所有BiArticle
    /// </summary>
    Task<PageEntity<IEnumerable<BiArticle>>> getPagelist(PageEntity<BiArticleInput> inputs);
    /// <summary>
    /// 获取所有BiArticle
    /// </summary>
    Task<IEnumerable<BiArticle>> getList(BiArticleInput input);
    /// <summary>
    /// 根据ID获取单个BiArticle
    /// </summary>
    Task<(double,BiArticle)> getEntity(BiArticleInput input);
}

