using AutoMapper;
using Accounts.API.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Accounts.API.Common.Mapper;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog.Context;
using Serilog;
using Accounts.API.Common.Middlewares;
using Accounts.API.Common.Attributes;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using Accounts.API.Integrations.AwsS3;
using Hangfire;
using Hangfire.MemoryStorage;
using Accounts.API.Interfaces.Services;
using Hangfire.Common;
using Accounts.API.Common;

namespace Accounts.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly EnvironmentsBase environmentsBase;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            AwsS3Integration.Initialize(configuration);
            environmentsBase = new EnvironmentsBase(configuration);
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors();

            services.AddDependencyInjections();
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile<Dto2Entity>();
                config.AddProfile<Entity2Dto>();
                config.AddMaps(Assembly.GetEntryAssembly());
            });

            var mapper = mapperConfiguration.CreateMapper();

            services.AddSingleton<IMapper>(mapper);
            services.AddMemoryCache();
            services.AddControllers(options =>
            options.Filters.AddService<AuditAttribute>()
            )
                 .ConfigureApiBehaviorOptions(options =>
                 {
                     options.SuppressModelStateInvalidFilter = true;
                 }); 

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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Accounts.API", Version = "v1" });
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

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute {
	            Attempts = 1, DelaysInSeconds = new int[] { 5000 } });

            services.AddHangfire(configuration => configuration
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage());

            services.AddHangfireServer();

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Accounts.API v1"));
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
            //app.UseHttpsRedirection();


            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseHangfireDashboard();

             var jobActive = $"{environmentsBase.JOB_SEND_EMAIL_RESCISION}";

            if(jobActive == "true")
                ExecuteJobSendEmail();
        }

        public void ExecuteJobSendEmail()
        {
            
            var timeZoneOptions = new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById(@"Central Brazilian Standard Time")
            };
        
            var recurringJob = new RecurringJobManager();
            recurringJob.AddOrUpdate("JobSendEmail", Job.FromExpression<IAccountService>(s => s.JobNotificationEmail()),
             "* 5 * * 1-5",
            timeZoneOptions);
            
        }

    }
}
