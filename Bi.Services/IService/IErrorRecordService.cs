﻿using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.IService;

public interface IErrorRecordService : IDependency
{
    Task<double> insert(ErrorRecordInput input);
}
