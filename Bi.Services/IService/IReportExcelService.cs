using Bi.Core.Interfaces;
using Bi.Entities.Input;
using Bi.Entities.Response;
using MagicOnion;

namespace Bi.Services.IService;

public interface IReportExcelService : IDependency
{
    /// <summary>
    /// 添加报表
    /// </summary>
    UnaryResult<(bool res, string msg)> AddAsync(IEnumerable<ReportExcelInput> input);
    /// <summary>
    /// 根据报表编码查询详情
    /// </summary>
    UnaryResult<ReportExcelOutput> detailByReportCode(string reportCode, bool master = true);

    /// <summary>
    /// 修改excel 报表
    /// </summary>
    UnaryResult<(bool res, string msg)> ModifyAsync(ReportExcelInput input);

    /// <summary>
    /// excel 导出
    /// </summary>
    Task<(string, string)> exportExcel(ReportExcelInput input);

    /// <summary>
    /// 初次预览
    /// </summary>
    Task<ReportExcelOutput> firstPreview(ReportExcelInput input);
    /// <summary>
    /// 预览计算总数
    /// </summary>
    Task<ReportExcelOutput> countPreview(ReportExcelInput input);

    /// <summary>
    /// excel 预览
    /// </summary>
    UnaryResult<ReportExcelOutput> preview(ReportExcelInput input);
}

