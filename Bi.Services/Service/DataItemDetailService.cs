using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

internal class DataItemDetailService : IDataItemDetailService
{
    public Task<IEnumerable<DataItemDetailResponse>> GetListAsync(DataItemDetailQueryInput input)
    {
        throw new NotImplementedException();
    }
}
