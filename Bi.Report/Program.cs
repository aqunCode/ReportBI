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
// 配置文档
var configuration = builder.Configuration;
// 环境变量
var WebHostEnvironment = builder.Environment;


//设置ConfigHelper
ConfigHelper.SetConfiguration(configuration);

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services

    //注入StackExchangeRedis
    .AddIf(true,
        x => x.AddStackExchangeRedis(configuration, true))

    //注入Redis分布式锁
    //.AddDistributedLock()

    //注入RabbitMq
    //.AddRabbitMq(configuration)

    //添加跨域支持
    .AddPolicyCors()

    //路由小写
    .AddRouting(options => options.LowercaseUrls = true)

    //注入静态资源配置
    .AddIf(configuration.GetValue<bool?>("AssetsOptions:Enabled") != false,
        x => x.Configure<List<AssetsOptions>>(configuration.GetSection("AssetsOptions")))

    //注入HttpClientFactory
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

    //添加ApiVersion版本控制
    .AddApiVersion()

    //添加Swagger接口文档
    .AddSwagger(configuration)

    //注入Mapster对象快速映射
    .AddMapster()

    //获取IP
    .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()

    //添加Controller并自动添加上忽略版本号设置
    .AddControllers(x =>
            {
                x.Filters.Add<ValidateModelFilter>();
                x.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            }
        ) //全局注入过滤器
    .AddApiVersionNeutral()
    .AddNewtonsoftJson(x =>
        x.SerializerSettings.ContractResolver = new JsonIgnoreAttributeContractResolver())
    .ConfigureApiBehaviorOptions(x =>
        x.SuppressModelStateInvalidFilter = true);

builder.Services
    //扫描程序集注入
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
    // 添加sqlsugar依赖
builder.Services.AddSqlSugarSetup(builder.Configuration,builder.Environment);



    //添加WatchDog
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
    //使用自定义策略跨域
    .UsePolicyCors()

    //全局异常处理
    .UseExceptionHandler(configuration, iLogger)

    //使用路由
    .UseRouting()

    //使用Redis信息
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
