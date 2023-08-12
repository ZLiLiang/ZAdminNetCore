using AspNetCoreRateLimit;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Z.Admin.WebApi.Extensions;
using Z.Admin.WebApi.Filters;
using Z.Admin.WebApi.Framework;
using Z.Admin.WebApi.Hubs;
using Z.Admin.WebApi.Middleware;
using Z.Common.Cache;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//ע��HttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// ��������
builder.Services.AddCors(builder.Configuration);
// ��ʾlogo
builder.Services.AddLogo();
//ע��SignalRʵʱͨѶ��Ĭ����json����
builder.Services.AddSignalR();
//����Error unprotecting the session cookie����
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "DataProtection"));
//��ͨ��֤��
builder.Services.AddCaptcha(builder.Configuration);
//IPRatelimit
builder.Services.AddIPRate(builder.Configuration);
builder.Services.AddHttpContextAccessor();
//����������Model��
builder.Services.Configure<OptionsSetting>(builder.Configuration);

//jwt ��֤
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = JwtUtil.ValidParameters();
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // ������ڣ��ѹ�����Ϣ��ӵ�ͷ��
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton(new AppSettings(builder.Configuration));
builder.Services.AddAppService();
//�����ƻ�����
builder.Services.AddTaskSchedulers();

//ע��REDIS ����
var openRedis = builder.Configuration["RedisServer:open"];
if (openRedis == "1")
{
    RedisServer.Initalize();
}

builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(GlobalActionMonitor));//ȫ��ע��
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonConverterUtil.DateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonConverterUtil.DateTimeNullConverter());
    });

builder.Services.AddSwaggerConfig();

var app = builder.Build();

InternalApp.ServiceProvider = app.Services;
InternalApp.Configuration = builder.Configuration;
InternalApp.WebHostEnvironment = app.Environment;
//��ʼ��db
builder.Services.AddDb(builder.Configuration, app.Environment);

//ʹ��ȫ���쳣�м��
app.UseMiddleware<GlobalExceptionMiddleware>();

app.Use((context, next) =>
{
    //���ÿ��Զ�λ�ȡbody����
    context.Request.EnableBuffering();
    if (context.Request.Query.TryGetValue("access_token", out var token))
    {
        context.Request.Headers.Add("Authorization", $"Bearer {token}");
    }
    return next();
});
//�������ʾ�̬�ļ�/wwwrootĿ¼�ļ���Ҫ����UseRoutingǰ��
app.UseStaticFiles();
//����·�ɷ���
app.UseRouting();
app.UseCors("Policy");//Ҫ����app.UseEndpointsǰ��

app.UseAuthentication();
app.UseAuthorization();

//��������
app.UseResponseCaching();
if (builder.Environment.IsProduction())
{
    //�ָ�/��������
    app.UseAddTaskSchedulers();
}

//ʹ��swagger
app.UseSwagger();

//���ÿͻ���IP��������
app.UseIpRateLimiting();
app.UseRateLimiter();
//����socket����
app.MapHub<MessageHub>("/msgHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
