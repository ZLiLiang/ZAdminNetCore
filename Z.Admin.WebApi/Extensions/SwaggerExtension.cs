using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace Z.Admin.WebApi.Extensions
{
    public static class SwaggerExtension
    {

        public static void UseSwagger(this IApplicationBuilder application)
        {
            application.UseSwagger(options =>
            {
                options.RouteTemplate = "swagger/{documentName}/swagger.json";
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var url = $"{httpReq.Scheme}://{httpReq.Host.Value}";
                    var referer = httpReq.Headers["Referer"].ToString();
                    if (referer.Contains(GlobalConstant.DevApiProxy))
                    {
                        url = referer[..(referer.IndexOf(GlobalConstant.DevApiProxy, StringComparison.InvariantCulture) + GlobalConstant.DevApiProxy.Length - 1)];
                        swaggerDoc.Servers = new List<OpenApiServer>
                        {
                            new OpenApiServer
                            {
                                Url= url,
                            }
                        };
                    }
                });
            });
            application.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("v1/swagger.json", "ZAdmin v1");
            });
        }

        public static void AddSwaggerConfig(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ZAdmin.NET Api",
                    Version = "v1",
                    Description = ""
                });
                try
                {
                    var baseDir = AppContext.BaseDirectory;
                    setupAction.IncludeXmlComments(Path.Combine(baseDir, "Z.Model.xml"), true);

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(baseDir, xmlFile);
                    setupAction.IncludeXmlComments(xmlPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("swagger 文档加载失败" + ex.Message);
                }

                //在header 中添加token，传递到后台
                setupAction.OperationFilter<SecurityRequirementsOperationFilter>();
                setupAction.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "请输入Login接口返回的Token，前置Bearer。示例：Bearer {Token}",
                    Name = "Authorization",//jwt默认的参数名称,
                    Type = SecuritySchemeType.ApiKey, //指定ApiKey
                    BearerFormat = "JWT",//标识承载令牌的格式 该信息主要是出于文档目的
                    Scheme = JwtBearerDefaults.AuthenticationScheme//授权中要使用的HTTP授权方案的名称
                });
                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                         new List<string>()
                    }
                });
            });
        }
    }
}
