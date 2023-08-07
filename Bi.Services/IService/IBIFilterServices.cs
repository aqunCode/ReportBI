using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IBIFilterServices:IDependency
{
    Task<(string,string)> BIFilter(BIWorkbookInput input);

    Task<(string,PageEntity<List<string>>)> selectValue(PageEntity<ColumnInfo> input);

}
