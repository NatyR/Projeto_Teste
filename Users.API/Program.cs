using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Oracle.Columns;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Extensions;

namespace Users.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var configSettings = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //    .Build();

            //Log.Logger = new LoggerConfiguration()
            //    .ReadFrom.Configuration(configSettings)
            //    .CreateLogger();

            //Serilog.Debugging.SelfLog.Enable(Console.Error);
            try
            {
                var host = CreateHostBuilder(args).Build();

                Log.Information("Iniciando aplicação Users.API");

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Erro executando aplicação Users.API");
            }
            finally
            {
                Log.Information("Finalizando aplicação Users.API");
                Log.CloseAndFlush();
            }


        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var configSettings = config.Build();

                    
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.WithProperty("SERVICE", "Users.API")
                        .ReadFrom.Configuration(configSettings)
                        //.WriteTo.Oracle(cfg => 
                        //cfg.WithSettings(configSettings.GetConnectionString("PortalConnection"),"PORTALRH.T_LOGS", "PORTALRH.get_log_seq")
                        //.UseBurstBatch()
                        //.CreateSink())
                        .CreateLogger();
                })
                 .UseSerilog()
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder
                     .UseStartup<Startup>();
                 });
        }
    }
}
