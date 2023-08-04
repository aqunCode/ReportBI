using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Bi.Report.Controllers.DynamicCode;

[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "bireport")]
[Route("[controller]/[action]")]
public class DynamicCodeController :BaseController
    {
        /// <summary>
        /// datasource 服务接口
        /// </summary>
        private readonly IDynamicCodeService service;

        /// <summary>
        /// datasource 构造函数
        /// </summary>
        public DynamicCodeController(IDynamicCodeService service)
        {
            this.service = service;
        }

        [HttpPost]
        [ActionName("syntaxRules")]
        public async Task<ResponseResult> syntaxRules(DynamicCodeInput input)
        {
            input.CheckFlag = true;
            var message = await service.syntaxRules(input);
            return Success(message.Item1);
        }
    }

