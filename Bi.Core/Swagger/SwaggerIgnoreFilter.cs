using System.Linq;
using System.Reflection;
using Bi.Core.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using Bi.Core.Extensions;

namespace Bi.Core.Swagger
{
    /// <summary>
    /// 排除输出参数(Schema)实体中部分字段或属性
    /// </summary>
    public class SwaggerIgnoreSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// 实现ISchemaFilter接口
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties.Count == 0)
                return;

            const BindingFlags bindingFlags = BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance;
            var memberList = context.Type
                                .GetFields(bindingFlags).Cast<MemberInfo>()
                                .Concat(context.Type
                                .GetProperties(bindingFlags));

            var excludedList = memberList
                                    .Where(m =>
                                        m.GetCustomAttribute<SwaggerIgnoreAttribute>() != null)
                                    .Select(m =>
                                        m.GetCustomAttribute<JsonPropertyAttribute>()
                                        ?.PropertyName
                                        ?? m.Name.ToCamelCase());

            foreach (var excludedName in excludedList)
            {
                if (schema.Properties.ContainsKey(excludedName))
                    schema.Properties.Remove(excludedName);
            }
        }
    }

    /// <summary>
    /// 排除输入参数实体中部分字段或者属性
    /// </summary>
    public class SwaggerIgnoreOperationFilter : IOperationFilter
    {
        /// <summary>
        /// 实现IOperationFilter接口
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //根据SwaggerIgnore特性排除指定参数的属性
            var ignores = context.ApiDescription.ParameterDescriptions
                            .Where(x =>
                                (x.ModelMetadata as DefaultModelMetadata)?.Attributes?.PropertyAttributes?
                            .Any(x =>
                                x.GetType() == typeof(SwaggerIgnoreAttribute)) == true);

            if (ignores?.Count() > 0)
            {
                var parameters = (from a in ignores
                                  join b in operation.Parameters on a.Name equals b.Name
                                  select b).ToArray();

                if (parameters?.Length > 0)
                    operation.Parameters.RemoveRange(parameters);
            }
        }
    }
}
