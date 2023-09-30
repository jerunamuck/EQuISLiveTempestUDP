using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EQuISLiveTempestUDP;

public class Program
{
    public static void Main(string[] args)
    {
        ConfigurationBuilder configBldr = new();
        BuildConfig(configBldr);
        IConfigurationRoot config = configBldr.Build();

        Log.Logger = new LoggerConfiguration()
          .ReadFrom.Configuration(config)
          .Enrich.FromLogContext()
          .WriteTo.Console()
          .WriteTo.File(path: "EQuISLiveTempest.log", rollingInterval:RollingInterval.Day )
          .CreateLogger();

        var host = Host.CreateDefaultBuilder()
        .UseWindowsService()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton<EQuISLiveClient>();
            services.AddHostedService<Worker>();
            services.AddLogging();
        })
        .UseSerilog()
        .Build();

        host.Run();
    }

    private static void BuildConfig(IConfigurationBuilder configBuilder)
    {
        configBuilder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json",optional:false, reloadOnChange:true)
            .Build();
    }

}

