using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using MagicOnion;

namespace Bi.Services.IService;

public interface IReportService :IDependency
{
    /// <summary>
    /// 添加报表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<(bool res, string msg)> AddAsync(IEnumerable<ReportInput> input);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="master"></param>
    /// <returns></returns>

    UnaryResult<IEnumerable<AutoReport>> GetEntityListAsync(ReportQueryInput input, bool master = true);
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>

    UnaryResult<PageEntity<IEnumerable<AutoReport>>> GetPageListAsync(PageEntity<ReportQueryInput> input);

    /// <summary>
    /// 修改 report 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<(bool res, string msg)> ModifyAsync(ReportInput input);

    /// <summary>
    /// 删除 report 信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    UnaryResult<double> deleteAsync(ReportQueryInput input);

    Task<double> DeleteAsync(string[] ids, CurrentUser currentUser);

}