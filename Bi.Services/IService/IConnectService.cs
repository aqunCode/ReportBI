using Bi.Core.Interfaces;
using Bi.Entities.Input;
using Bi.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.IServicep;

public interface IConnectService : IDependency
{
    Task<TokenResponse> getToken(UserInfo input);
}
