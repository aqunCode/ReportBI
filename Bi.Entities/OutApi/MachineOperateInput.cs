using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Entities.OutApi;

public class MachineOperateInput : BaseInput
{
    /// <summary>
    /// 工号
    /// </summary>
    public string? EmployeeCard { get; set; }
    /// <summary>
    /// 姓名
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 岗位
    /// </summary>
    public string? Post { get; set; }
    /// <summary>
    /// mac地址
    /// </summary>
    public string? PhoneMac { get; set; }
    /// <summary>
    /// 设备编码
    /// </summary>
    public string? MachineCode { get; set; }
    /// <summary>
    /// 日期
    /// </summary>
    public string? DateStr {  get; set; }
    /// <summary>
    /// 备用
    /// </summary>
    public string? Opt1 { get; set; }
    /// <summary>
    /// 备用
    /// </summary>
    public string? Opt2 { get; set; }
    /// <summary>
    /// 备用
    /// </summary>
    public string? Opt3 { get; set; }
}

