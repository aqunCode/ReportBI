using Bi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.IServices;

public interface ITestServices : IDependency
{
    /// <summary>
    /// 测试
    /// </summary>
    /// <returns></returns>
    public Task<DataTable> queryAll();
}

