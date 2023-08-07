using Bi.Core.Interfaces;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IAggregationServices : IDependency
{
    (string, string) AnalysisAggregate(BIWorkbookInput input);

    Task<(string, string)> AnalysisCalcSql(BIWorkbookInput input);
}
