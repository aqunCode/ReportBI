using Bi.Core.Interfaces;
using Bi.Entities.OutApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.IOutService;

public interface IMachineOperateService : IDependency
{
    Task<int> addAsync(MachineOperateInput input);
}
