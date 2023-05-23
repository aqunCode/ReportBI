using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Redis;
/// <summary>
/// A class that contains redis connection pool informations.
/// </summary>
public class ConnectionPoolInformation
{
    /// <summary>
    /// Gets or sets the connection pool desiderated size.
    /// </summary>
    public int RequiredPoolSize { get; set; }

    /// <summary>
    /// Gets or sets the number of active connections in the connection pool.
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Gets or sets the hash code of active connections in the connection pool.
    /// </summary>
    public List<int> ActiveConnectionHashCodes { get; set; }

    /// <summary>
    /// Gets or sets the number of invalid connections in the connection pool.
    /// </summary>
    public int InvalidConnections { get; set; }

    /// <summary>
    /// Gets or sets the hash code of invalid connections in the connection pool.
    /// </summary>
    public List<int> InvalidConnectionHashCodes { get; set; }
}