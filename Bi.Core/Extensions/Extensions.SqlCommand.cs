﻿using Microsoft.Data.SqlClient;
using System.Data;

namespace Bi.Core.Extensions
{
    /// <summary>
    /// SqlCommand扩展类
    /// </summary>
    public static class SqlCommandExtensions
    {
        #region ExecuteDataSet
        /// <summary>
        /// Executes the query, and returns the result set as DataSet.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DataSet that is equivalent to the result set.</returns>
        public static DataSet ExecuteDataSet(this SqlCommand @this)
        {
            var ds = new DataSet();
            using (var dataAdapter = new SqlDataAdapter(@this))
            {
                dataAdapter.Fill(ds);
            }

            return ds;
        }
        #endregion

        #region ExecuteDataTable
        /// <summary>
        /// Executes the query, and returns the first result set as DataTable.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DataTable that is equivalent to the first result set.</returns>
        public static DataTable ExecuteDataTable(this SqlCommand @this)
        {
            var dt = new DataTable();
            using (var dataAdapter = new SqlDataAdapter(@this))
            {
                dataAdapter.Fill(dt);
            }

            return dt;
        }
        #endregion
    }
}
