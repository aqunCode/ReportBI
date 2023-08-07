using Bi.Core.Interfaces;
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
    Task<IEnumerable<DataItemDetailResponse>> GetListAsync(DataItemDetailQueryInput input);
}
