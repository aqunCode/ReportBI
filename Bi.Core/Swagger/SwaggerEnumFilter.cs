using Bi.Core.Extensions;
using Bi.Core.Helpers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Bi.Core.Swagger
{
    /// <summary>
    /// 枚举类型属性过滤
    /// </summary>
    public class SwaggerEnumFilter : IDocumentFilter
    {
        /// <summary>
        /// 实现IDocumentFilter接口
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var dict = GetAllEnums();

            foreach (var item in swaggerDoc.Components.Schemas)
            {
                var property = item.Value;
                var typeName = item.Key;
                if (property.Enum?.Count > 0)
                {
                    Type itemType = null;
                    if (dict != null && dict.ContainsKey(typeName))
                        itemType = dict[typeName];

                    var list = new List<OpenApiInteger>();
                    foreach (var val in property.Enum)
                    {
                        list.Add((OpenApiInteger)val);
                    }

                    property.Description += DescribeEnum(itemType, list);
                }
            }
        }

        /// <summary>
        /// 获取所有枚举类型
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Type> GetAllEnums()
        {
            var assemblies = AssemblyHelper.GetAssemblies(filter: x => x.StartsWithIgnoreCase("Baize", "SQLBuilder.Core"));

            var retval = new Dictionary<string, Type>();
            assemblies.ForEach(assembly =>
            {
                var types = assembly.GetTypes().Where(x => x.IsEnum);
                if (types.IsNotNullOrEmpty())
                    types.ForEach(item => retval[item.Name] = item);
            });

            return retval;
        }

        /// <summary>
        /// 枚举描述
        /// </summary>
        /// <param name="type"></param>
        /// <param name="enums"></param>
        /// <returns></returns>
        private static string DescribeEnum(Type type, List<OpenApiInteger> enums)
        {
            var enumDescriptions = new List<string>();

            foreach (var item in enums)
            {
                if (type == null)
                    continue;

                var value = Enum.Parse(type, item.Value.ToString());

                //获取枚举属性描述
                var desc = type
                            .GetMembers()
                            .Where(x => x.Name == type.GetEnumName(value))
                            .FirstOrDefault()?
                            .GetCustomAttribute<DescriptionAttribute>()?
                            .Description;

                if (string.IsNullOrEmpty(desc))
                    enumDescriptions.Add($"{item.Value}:{Enum.GetName(type, value)}; ");
                else
                    enumDescriptions.Add($"{item.Value}:{Enum.GetName(type, value)},{desc}; ");

            }

            return $"<br/>{Environment.NewLine}{string.Join("<br/>" + Environment.NewLine, enumDescriptions)}";
        }
    }
}
