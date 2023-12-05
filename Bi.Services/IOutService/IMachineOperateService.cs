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
    /// <summary>
    /// 添加职工上线信息
    /// </summary>
    Task<int> addAsync(MachineOperateInput input);
    /// <summary>
    /// 获取职工岗位信息
    /// </summary>
    Task<string> getPost(MachineOperateInput input);
}
