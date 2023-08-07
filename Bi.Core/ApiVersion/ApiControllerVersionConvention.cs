using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Collections.Generic;
using System.Reflection;

namespace Bi.Core.ApiVersion
{
    /// <summary>
    /// 自动添加ApiVersionNeutral特性，用于忽略版本号
    /// </summary>
    public class ApiControllerVersionConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (!(controller.ControllerType.IsDefined(typeof(ApiVersionAttribute)) || 
                controller.ControllerType.IsDefined(typeof(ApiVersionNeutralAttribute))))
            {
                if (controller.Attributes is List<object> attributes)
                    attributes.Add(new ApiVersionNeutralAttribute());
            }
        }
    }
}
