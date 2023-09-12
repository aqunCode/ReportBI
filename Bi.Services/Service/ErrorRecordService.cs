using Bi.Core.Const;
using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

internal class ErrorRecordService : IErrorRecordService
{
    public async Task<double> insert(ErrorRecordInput input)
    {
        return BaseErrorCode.Successful;
    }
}
