using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Bi.Core.Filters
{
    /// <summary>
    /// 输入参数模型校验全局过滤器
    /// </summary>
    public class ValidateModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var result = context.ModelState.Keys
                        .Where(key => !key.IsNullOrEmpty())
                        .SelectMany(key => context.ModelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage)))
                        .ToList();
                var response = new ResponseResult<List<ValidationError>>(ResponseCode.Error, result)
                {
                    ErrorCode = BaseErrorCode.Invalid_Input
                };
                context.Result = new ObjectResult(response);
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }

    /// <summary>
    /// 校验错误
    /// </summary>
    public class ValidationError
    {
        public string Field { get; }
        public string Message { get; }
        public ValidationError(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
    }
}