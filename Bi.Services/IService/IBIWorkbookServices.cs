using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using System.Data;

namespace Bi.Services.IService;

public interface IBIWorkbookServices : IDependency
{
    Task<string> addAsync(BIWorkbookInput input);

    Task<PageEntity<IEnumerable<BIWorkbook>>> getEntityListAsync(PageEntity<BIWorkbookInput> inputs);

    Task<(string,BIWorkbookInput)> getEchoAsync(BIWorkbookInput input);

    Task<string> deleteAsync(BIWorkbookInput input);

    Task<string> ModifyAsync(BIWorkbookInput input);

    Task<(string, DataTable)> previewAsync(BIWorkbookInput inputs);

    //Task<string> copyAsync(BIWorkbookInput input);

}
