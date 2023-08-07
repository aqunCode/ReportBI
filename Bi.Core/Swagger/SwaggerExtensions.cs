using Bi.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bi.Core.Swagger
{
    /// <summary>
    /// Swagger扩展类
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// AddSwagger扩展方法
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwagger(this IServiceCollection @this, IConfiguration configuration)
        {
            //判断是否启用Swagger
            if (configuration.GetValue<bool>("Swagger:Enabled"))
            {
                //添加Swagger
                @this.AddSwaggerGen(options =>
                {
                    var docNames = configuration.GetSection("Swagger:DocNames").Get<string[]>();

                    var provider = @this.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var desc in provider.ApiVersionDescriptions)
                    {
						foreach (var docName in docNames)
						{
                            options.SwaggerDoc($"v{desc.ApiVersion}-{docName}", new OpenApiInfo
                            {
                                Version = $"v{desc.ApiVersion}",
                                Title = $"v{desc.ApiVersion}-{docName}",
                                Description = configuration.GetValue<string>("Swagger:Description"),
                            });
                        }
                    }

                    //Doc条件
                    options.DocInclusionPredicate((docName, apiDesc) =>
                    {
                        //Version版本
                        var versions = apiDesc
                                            .CustomAttributes()
                                            .OfType<ApiVersionAttribute>()
                                            .SelectMany(attr => attr.Versions);

                        //MapToVersion版本
                        var mapToVersions = apiDesc
                                            .CustomAttributes()
                                            .OfType<MapToApiVersionAttribute>()
                                            .SelectMany(attr => attr.Versions)
                                            .DefaultIfNull();

                        //Api分组
                        var groups = apiDesc
                                    .CustomAttributes()
                                    .OfType<ApiExplorerSettingsAttribute>()
                                    .Select(x => x.GroupName);

                        var version = docName.Split("-")[0];
                        var groupName = docName.Split("-")[1];

                        return
                            groups.Any(x => x == groupName) &&
                            versions.Any(v => $"v{v}" == version) && (apiDesc.RelativePath.ContainsIgnoreCase(version) || !Regex.IsMatch(apiDesc.RelativePath.ToLower(),"/v[0-9]+/")) &&
                            (mapToVersions.IsNotNullOrEmpty()
                                ? mapToVersions.Any(v => $"v{v}" == version)
                                : version == "v1");
                    });

                    //添加权限
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            Array.Empty<string>()
                        }
                    });

                    //设置Servers
                    options.AddServer(new OpenApiServer
                    {
                        Url = configuration.GetValue<string>("Swagger:Server") ?? configuration.GetValue<string>("Urls")
                    });

                    //排除输出实体部分属性
                    options.SchemaFilter<SwaggerIgnoreSchemaFilter>();

                    //排除输入参数部分属性
                    options.OperationFilter<SwaggerIgnoreOperationFilter>();

                    //设置枚举类型过滤器
                    options.DocumentFilter<SwaggerEnumFilter>();

                    //xml注释
                    var files = Directory.GetFiles(AppContext.BaseDirectory, "*.xml").ToList();
                    files.ForEach(x => options.IncludeXmlComments(x, true));
                });
            }

            return @this;
        }

        /// <summary>
        /// UseSwagger扩展方法
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder @this, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("Swagger:Enabled"))
            {
                @this.UseSwagger(c =>
                {
                    c.RouteTemplate = "{documentName}/swagger.json";
                });

                @this.UseSwaggerUI(options =>
                {
                    var docNames = configuration.GetSection("Swagger:DocNames").Get<string[]>();

                    var provider = @this.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                    foreach (var desc in provider.ApiVersionDescriptions)
                    {
						foreach (var docName in docNames)
						{
                            options.SwaggerEndpoint(
                                $"/v{desc.ApiVersion}-{docName}/swagger.json",
                               $"v{desc.ApiVersion}-{docName}");
                        }

                    }
                });
            }

            return @this;
        }
    }
}
