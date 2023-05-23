using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Bi.Core.SqlSugar;
using Bi.Core.Extensions;
using Bi.Core.Interfaces;
using Bi.Core.Json;
using Bi.Core.ApiVersion;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersion();

// 添加sqlsugar依赖
builder.Services.AddSqlSugarSetup(builder.Configuration,builder.Environment);
//builder.Services.AddSqlSugarSetup(Configuration, WebHostEnvironment);
//注入自定义服务
builder.Services
    //扫描程序集注入
    .AddFromAssembly(
        type => type
            .AssignableTo<IDependency>()
            .Where(x =>
                x.Namespace.StartsWithIgnoreCase(
                    "Bi.ReportDesign",
                    "Bi.Services",
                    "Bi.Entity",
                    "Bi.Core")),
        assembly => assembly
            .StartsWithIgnoreCase("Bi."));
// 添加输入输出序列化功能,对接口返回的数据自动序列化
builder.Services.AddMvc().AddNewtonsoftJson(x =>
                    x.SerializerSettings.ContractResolver = new JsonIgnoreAttributeContractResolver())
                         // 添加swagger版本控制
                         .AddApiVersionNeutral();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
