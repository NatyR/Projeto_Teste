using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portal.API.Interfaces.Repositories;
using Portal.API.Repositories;
using Portal.API.Interfaces.Services;
using Portal.API.Service;
using Portal.API.Integrations.Interfaces;
using Portal.API.Common.Helpers.Interfaces;
using Portal.API.Common.Helpers;
using Portal.API.Common.Repositories.Interfaces;
using Portal.API.Common.Repositories;
using Portal.API.Common.Services;
using Portal.API.Integrations.Ploomes;
using System.Text;
using Portal.API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using Portal.API.Common.Mapper;
using System.Reflection;
using Portal.API.Common.Extensions;
using Serilog.Context;
using Serilog;
using Portal.API.Common.Middlewares;
using Portal.API.Common.Attributes;
using Portal.API.Integrations.AwsS3;

namespace Users.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            AwsS3Integration.Initialize(configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddDependencyInjections();

            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile<Dto2Entity>();
                config.AddProfile<Entity2Dto>();
                config.AddMaps(Assembly.GetEntryAssembly());
            });

            var mapper = mapperConfiguration.CreateMapper();

            services.AddSingleton<IMapper>(mapper);
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddResponseCaching();
            services.AddMemoryCache();
            services.AddControllers(options =>
            options.Filters.AddService<AuditAttribute>()
            )
                 .ConfigureApiBehaviorOptions(options =>
                 {
                     options.SuppressModelStateInvalidFilter = true;
                 });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portal.API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n Enter 'Bearer'[space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddDistributedMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portal.API v1"));
            }
            app.UseMiddleware<EnableRequestBodyBufferingMiddleware>();
            app.Use(async (ctx, next) =>
            {
                using (LogContext.PushProperty("IPAddress", ctx.Connection.RemoteIpAddress))
                {
                    using (LogContext.PushProperty("Headers", ctx.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())))
                    {
                        await next();
                    }
                }


            });
            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true) // allow any origin
               .AllowCredentials()); // allow credentials
            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
