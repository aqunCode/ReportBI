using Bi.Core.SqlSugar;
using Bi.Core.Extensions;
using Bi.Core.Interfaces;
using Bi.Core.Json;
using Bi.Core.ApiVersion;
using Bi.Core.Ftp;
using Bi.Core.Helpers;
using Bi.Core.Filters;
using Bi.Core.Models;
using System.Net.Security;
using Bi.Core.Cors;
using Bi.Core.Swagger;
using Bi.Core.Mapster;
using WatchDog;
using WatchDog.src.Enums;
using Bi.Core.Middleware;
using NLog.Extensions.Logging;
using NLog.Web;
using NLog;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

var builder = WebApplication.CreateBuilder(args);
// �����ĵ�
var configuration = builder.Configuration;
// ��������
var WebHostEnvironment = builder.Environment;


//����ConfigHelper
ConfigHelper.SetConfiguration(configuration);

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services

    //ע��StackExchangeRedis
    .AddIf(true,
        x => x.AddStackExchangeRedis(configuration, true))

    //ע��Redis�ֲ�ʽ��
    //.AddDistributedLock()

    //ע��RabbitMq
    //.AddRabbitMq(configuration)

    //��ӿ���֧��
    .AddPolicyCors()

    //·��Сд
    .AddRouting(options => options.LowercaseUrls = true)

    //ע�뾲̬��Դ����
    .AddIf(configuration.GetValue<bool?>("AssetsOptions:Enabled") != false,
        x => x.Configure<List<AssetsOptions>>(configuration.GetSection("AssetsOptions")))

    //ע��HttpClientFactory
    .AddIf(true,
        x => x
            .AddHttpClient("bi")
            .ConfigurePrimaryHttpMessageHandler(() =>
                new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    EnableMultipleHttp2Connections = true,
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback =
                            (request, certificate, chain, errors) => true
                    }
                }))

    //���ApiVersion�汾����
    .AddApiVersion()

    //���Swagger�ӿ��ĵ�
    .AddSwagger(configuration)

    //ע��Mapster�������ӳ��
    .AddMapster()

    //��ȡIP
    .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()

    //���Controller���Զ�����Ϻ��԰汾������
    .AddControllers(x =>
            {
                x.Filters.Add<ValidateModelFilter>();
                x.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            }
        ) //ȫ��ע�������
    .AddApiVersionNeutral()
    .AddNewtonsoftJson(x =>
        x.SerializerSettings.ContractResolver = new JsonIgnoreAttributeContractResolver())
    .ConfigureApiBehaviorOptions(x =>
        x.SuppressModelStateInvalidFilter = true);

builder.Services
    //ɨ�����ע��
    .AddFromAssembly(
        type => type
            .AssignableTo<IDependency>()
            .Where(x =>
                x.Namespace.StartsWithIgnoreCase(
                    "Bi.Report",
                    "Bi.Services",
                    "Bi.Entities",
                    "Bi.Core")),
        assembly => assembly
            .StartsWithIgnoreCase("Bi."))
                .AddFtpClient(builder.Configuration);

builder.Services.AddHealthChecks();
    // ���sqlsugar����
builder.Services.AddSqlSugarSetup(builder.Configuration,builder.Environment);



    //���WatchDog
builder.Services.AddWatchDogServices(opt =>
 {
     opt.IsAutoClear = true;
     opt.ClearTimeSchedule = WatchDogAutoClearScheduleEnum.Weekly;
 });

App.Services = builder.Services;



var app = builder.Build();

App.RootServices = app.Services;

var lifeTime = app.Services.GetService<IHostApplicationLifetime>();

var iLogger = app.Services.GetService<Microsoft.Extensions.Logging.ILogger>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(configuration);
    app.UseDeveloperExceptionPage();
    //app.UseExceptionHandler("/Home/Error");
    //app.UseHsts();
}

app
    //ʹ���Զ�����Կ���
    .UsePolicyCors()

    //ȫ���쳣����
    .UseExceptionHandler(configuration, iLogger)

    //ʹ��·��
    .UseRouting()

    //ʹ��Redis��Ϣ
    //.UseRedisInformation(configuration)

    //IP
    .UseHttpContext();

    // WatchDog
app.UseWatchDog(opt =>
{
    opt.WatchPageUsername = "admin";
    opt.WatchPagePassword = "Qwerty@123";
});

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("health");
    endpoints.MapControllers();
});

NLog.LogManager.Shutdown();
app.Run();
