using Oracle.ManagedDataAccess.Client;
using System;

namespace Bi.Core.Attributes
{
    /// <summary>
    /// Oracle列特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class OracleColumnAttribute : Attribute
    {
        /// <summary>
        /// Oracle数据类型
        /// </summary>
        public OracleDbType DbType { get; set; }
    }
}
