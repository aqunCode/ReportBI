using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Helpers
{
    public class Sys
    {
        public static string Guid
        {
            get
            {
                return System.Guid.NewGuid().ToString("N").ToUpper();
            }
        }
    }
}
