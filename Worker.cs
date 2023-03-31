namespace EQuISLiveTempestUDP;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //     await Task.Delay(1000, stoppingToken);
        // }
        await WeatherFlowUdpListener.WFListener
        .Create()
        .OnReceiveObservationTempestMessage(msg =>
        {
            Console.WriteLine(msg);
        })
        .OnReceiveObservationAirMessage(msg =>
        {
            Console.WriteLine($"Air-Temperature: {msg.AirTemperature}");
        })
        .OnReceiveObservationSkyMessage(msg =>
        {
            Console.WriteLine($"UV-Index: {msg.UV}");
        })
        .OnReceiveRapidWindMessage(msg =>
        {
            Console.WriteLine($"Wind-Speed: {msg.WindSpeed}");
        })
        .ListenAsync(stoppingToken);
    }
    
    public override Task StartAsync(CancellationToken token)
    {
        _logger.LogInformation("Background service starting.");
        return base.StartAsync(token);
    }

    public override Task StopAsync(CancellationToken token)
    {
        _logger.LogInformation("Background service stopping.");
        return base.StopAsync(token);
    }

}
