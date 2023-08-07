using Amazon;
using Grpc.Core;
using System.Data;

namespace Bi.Core.Helpers;

public class DataTableHelper
{
    public int status = 0;

    public DataTable dt;

    private OrderedEnumerableRowCollection<DataRow>? orderRow;

    public DataTableHelper(DataTable dt)
    {
        this.dt = dt;
    }

    public void Sortby(string columnName,string orderType, string[] SortList = null)
    {
        orderType = orderType.ToLower();
        switch (status, orderType) {
            case (0,"asc"):
                orderRow = dt.AsEnumerable().OrderBy(row => row.Field<int>(columnName));
                status = 1;
                break;
            case (0, "desc"):
                orderRow = dt.AsEnumerable().OrderByDescending(row => row.Field<int>(columnName));
                status = 1;
                break;
            case (1, "asc"):
                orderRow = orderRow.ThenBy(row => row.Field<int>(columnName));
                break;
            case (1, "desc"):
                orderRow = orderRow.ThenByDescending(row => row.Field<int>(columnName));
                break;
            case (0, "manual"):
                orderRow = dt.AsEnumerable().OrderBy(row => Array.IndexOf(SortList, row.Field<string>(columnName)));
                status = 1;
                break;
            case (1, "manual"):
                orderRow = orderRow.ThenBy(row => Array.IndexOf(SortList, row.Field<string>(columnName)));
                break;
        }
    }

    public DataTable GetValue()
    {
        return orderRow.CopyToDataTable();
    }
}

