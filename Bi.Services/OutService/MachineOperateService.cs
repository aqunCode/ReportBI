using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using Bi.Entities.Entity;
using Bi.Entities.OutApi;
using Bi.Services.IOutService;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.OutService;

internal class MachineOperateService : IMachineOperateService
{
    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScope scope;

    public MachineOperateService(ISqlSugarClient _sqlSugarClient)
    {

        scope = _sqlSugarClient as SqlSugarScope;
    }

    public async Task<int> addAsync(MachineOperateInput input)
    {
        var repository = scope.GetConnectionScope("bidb");
        MachineOperate mo = input.MapTo<MachineOperate>();
        mo.Id = Sys.Guid;
        mo.CreateDate = DateTime.Now;
        return await repository.Insertable<MachineOperate>(mo).ExecuteCommandAsync();
    }

    public async Task<string> getPost(MachineOperateInput input)
    {
        var repository = scope.GetConnectionScope("oadb");
        var opt = await repository.SqlQueryable<MachineOperate>($@"select a.loginid employeecard,
                                                    a.lastname name, 
                                                    b.jobtitlename post  from HrmResource a 
                                                    left join HrmJobTitles b on a.jobtitle  = b.id 
                                                    where a.loginid = '{input.EmployeeCard}' and a.status <> 5 ").FirstAsync();
        if(opt != null && opt.Post!=null)
        {
            return opt.Post;
        }
        return "ERROR 用户信息查询失败";
    }
}

