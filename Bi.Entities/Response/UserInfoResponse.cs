using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.Response;

public class TokenResponse:ResponseResult<CurrentUserResponse>
{
    public double Code { get; set; }
    public string? Access_token { get; set; }
    public string? Refresh_token { get; set; }
}
