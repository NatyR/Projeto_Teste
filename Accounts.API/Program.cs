using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();

                Log.Information("Iniciando aplicação Accounts.API");

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Erro executando aplicação Accounts.API");
            }
            finally
            {
                Log.Information("Finalizando aplicação Accounts.API");
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
                        .Enrich.WithProperty("SERVICE", "Accounts.API")
                        .ReadFrom.Configuration(configSettings)
                        .CreateLogger();
                })
                 .UseSerilog()
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder
                     .UseStartup<Startup>()
                     .UseKestrel(o =>
                     {
                         o.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                         o.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(15);
                         o.Limits.MaxRequestBodySize = long.MaxValue;
                     });
                 });
        }

    }
}
