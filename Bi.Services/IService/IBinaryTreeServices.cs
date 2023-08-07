using Bi.Core.Interfaces;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IBinaryTreeServices : IDependency
{
    Task<(string,string)> AnalysisRelation(BIWorkbookInput input);
    
}
