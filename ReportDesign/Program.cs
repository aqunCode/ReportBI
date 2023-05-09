using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Bi.Core.SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ÃÌº”sqlsugar“¿¿µ
builder.Services.AddSqlSugarSetup(builder.Configuration,builder.Environment);
//builder.Services.AddSqlSugarSetup(Configuration, WebHostEnvironment);

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
