using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Users.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();

                Log.Information("Iniciando aplicação Portal.API");

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Erro executando aplicação Portal.API");
            }
            finally
            {
                Log.Information("Finalizando aplicação Portal.API");
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
                        .Enrich.WithProperty("SERVICE", "Portal.API")
                        .ReadFrom.Configuration(configSettings)
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
