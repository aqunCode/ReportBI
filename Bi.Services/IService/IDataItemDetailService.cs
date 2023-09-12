using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.IService;

public interface IDataItemDetailService : IDependency
{
    Task<double> insertTree(DataItemInput input);
    Task<double> deleteTree(DataItemInput input);
    Task<double> modifyTree(DataItemInput input);
    Task<double> insert(DataItemDetailQueryInput input);
    Task<double> delete(DataItemDetailQueryInput input);
    Task<double> modify(DataItemDetailQueryInput input);
    Task<IEnumerable<DataItemTree>> getDataDictTree();
    Task<IEnumerable<DataItemDetailResponse>> GetListAsync(DataItemDetailQueryInput input);
    Task<PageEntity<IEnumerable<DataItemDetailResponse>>> getPagelist(PageEntity<DataItemDetailQueryInput> inputs);
    
}
