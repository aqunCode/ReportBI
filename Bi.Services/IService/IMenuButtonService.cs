using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.IService;

public interface IMenuButtonService : IDependency
{
    Task<IEnumerable<AuthMenuResponse>> GetListTreeCurrentUserAsync(CurrentUser currentUser);
}
