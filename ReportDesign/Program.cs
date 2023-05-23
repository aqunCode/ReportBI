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

// ���sqlsugar����
builder.Services.AddSqlSugarSetup(builder.Configuration,builder.Environment);
//builder.Services.AddSqlSugarSetup(Configuration, WebHostEnvironment);
//ע���Զ������
builder.Services
    //ɨ�����ע��
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
// �������������л�����,�Խӿڷ��ص������Զ����л�
builder.Services.AddMvc().AddNewtonsoftJson(x =>
                    x.SerializerSettings.ContractResolver = new JsonIgnoreAttributeContractResolver())
                         // ���swagger�汾����
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
