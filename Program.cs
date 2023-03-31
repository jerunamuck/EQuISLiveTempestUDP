using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EQuISLiveTempestUDP
{

    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigureLogger(args);
            IHost host = CreateHostBuilder(args).Build();
            host.Run();
        }

        private static void ConfigureLogger(string[] args)
        {
            // var configBuilder = new ConfigurationBuilder()
            //     .AddJsonFile("appsettings.json")
            //     .Build();
            LoggerConfiguration cfg = new LoggerConfiguration()
                .WriteTo.Console();
             //   .ReadFrom.Configuration(configBuilder)
             //   .WriteTo.EventLog("EQuISLive TempestUDP Agent",manageEventSource:true);

            //if(args.Contains("-v") || args.Contains("-V"))
            //{
            //    cfg.WriteTo.Console();
            //}
            Log.Logger = cfg.CreateLogger();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}

/* From template
using EQuISLiveTempestUDP;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync(); */
