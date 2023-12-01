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
    private SqlSugarScopeProvider repository;

    public MachineOperateService(ISqlSugarClient _sqlSugarClient)
    {

        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
    }

    public async Task<int> addAsync(MachineOperateInput input)
    {
        MachineOperate mo = input.MapTo<MachineOperate>();
        mo.Id = Sys.Guid;
        mo.CreateDate = DateTime.Now;
        return await repository.Insertable<MachineOperate>(mo).ExecuteCommandAsync();
    }
}

