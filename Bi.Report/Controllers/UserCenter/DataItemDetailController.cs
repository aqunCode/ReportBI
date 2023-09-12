using Bi.Entities.Entity;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Bi.Services.IServicep;
using Bi.Entities.Response;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using System.Net;
using Bi.Core.Models;
using Bi.Entities.Input;
using System.ComponentModel.DataAnnotations;

namespace Bi.Report.Controllers.UserCenter;

/// <summary>
/// �����ֵ���ϸ����
/// </summary>
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class DataItemDetailController : BaseController
{
    /// <summary>
    /// �ֶ�
    /// </summary>
    private readonly IDataItemDetailService _dataItemDetailService;

    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="dataItemDetailService"></param>
    public DataItemDetailController(IDataItemDetailService dataItemDetailService)
    {
        _dataItemDetailService = dataItemDetailService;
    }
    /// <summary>
    /// ���������ֵ���ϸ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insertTree")]
    public async Task<ResponseResult> insertTree(DataItemInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.insertTree(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }/// <summary>
     /// ɾ�������ֵ���ϸ
     /// </summary>
     /// <returns></returns>
    [HttpPost]
    [ActionName("deleteTree")]
    public async Task<ResponseResult> deleteTree(DataItemInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.deleteTree(input);
        if (result > 0)
            return Success("ɾ��ִ�гɹ�����ɾ��" + result + "��");
        else
            return Error("ɾ��ʧ�ܣ�");
    }/// <summary>
     /// �޸������ֵ���ϸ
     /// </summary>
     /// <returns></returns>
    [HttpPost]
    [ActionName("modifyTree")]
    public async Task<ResponseResult> modifyTree(DataItemInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.modifyTree(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }
    /// <summary>
    /// ���������ֵ���ϸ
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("insert")]
    public async Task<ResponseResult> insert(DataItemDetailQueryInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.insert(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }/// <summary>
     /// ɾ�������ֵ���ϸ
     /// </summary>
     /// <returns></returns>
    [HttpPost]
    [ActionName("delete")]
    public async Task<ResponseResult> delete(DataItemDetailQueryInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.delete(input);
        if (result > 0)
            return Success("ɾ��ִ�гɹ�����ɾ��" + result + "��");
        else
            return Error("ɾ��ʧ�ܣ�");
    }/// <summary>
     /// �޸������ֵ���ϸ
     /// </summary>
     /// <returns></returns>
    [HttpPost]
    [ActionName("modify")]
    public async Task<ResponseResult> modify(DataItemDetailQueryInput input)
    {
        input.CurrentUser = this.CurrentUser;
        double result = await _dataItemDetailService.modify(input);
        if (result > 0)
            return Success();
        else
            return Error();
    }


    /// <summary>
    /// ��ҳ��ȡ�����ֵ���ϸ�б�
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getPagelist")]
    public async Task<ResponseResult<PageEntity<IEnumerable<DataItemDetailResponse>>>> getPagelist(PageEntity<DataItemDetailQueryInput> inputs)
    {
        var data = await _dataItemDetailService.getPagelist(inputs);
        return Success(data);
    }

    /// <summary>
    /// ��ȡ�����ֵ���ϸ�б�
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ActionName("getlist")]
    public async Task<ResponseResult<IEnumerable<DataItemDetailResponse>>> GetListAsync(DataItemDetailQueryInput input)
    {
        var data = await _dataItemDetailService.GetListAsync(input);
        return Success(data);
    }

    /// <summary>
    /// ��ȡ�����ֵ���״�ṹ
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpGet]
    [ActionName("getDataDictTree")]
    public async Task<ResponseResult<IEnumerable<DataItemTree>>> getDataDictTree()
    {
        var res = await _dataItemDetailService.getDataDictTree();
        if (res.Count() > 0)
        {
            res = res
                    .TreeToJson("Id", new[] { "0" }, childName: "children")
                    .ToObject<IEnumerable<DataItemTree>>();
        }
        return Success(res);
    }

}