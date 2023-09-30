using System.Dynamic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using WeatherFlowUdpListener;

namespace EQuISLiveTempestUDP;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    JsonSerializerOptions jso = new JsonSerializerOptions();

    private EQuISLiveClient? _equis;
    public EQuISLiveClient? Equis { get => _equis; set => _equis = value; }

    private IConfiguration _config;
    public IConfiguration config {get => _config;}

    public Worker(ILogger<Worker> logger, IConfiguration config, EQuISLiveClient equis)
    {
        _config = config;
        _logger = logger;
        _equis = equis;
        jso.IncludeFields = true;
        jso.WriteIndented = true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool isIntialized = await _equis.Init();
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //     await Task.Delay(1000, stoppingToken);
        // }
        if(isIntialized)
        {
            await WeatherFlowUdpListener.WFListener
            .Create()
            .OnReceiveMessage(msg=>
            {
                List<LoggerDatum> datum = new List<LoggerDatum>();
                int seriesId = -1;
                Type mt = msg.GetType();
                switch(mt.Name)
                {
                    case "WFStatusDeviceMessage":
                        WFStatusDeviceMessage sdm = (WFStatusDeviceMessage)msg;
                        seriesId = Equis.GetSeriesId("device_status.uptime");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,sdm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),sdm.Uptime.GetValueOrDefault(TimeSpan.Zero).Seconds.ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("device_status.voltage");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,sdm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),sdm.Voltage.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("device_status.rssi");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,sdm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),sdm.RSSI.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("device_status.sensor_status");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,sdm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),sdm.SensorStatus.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("device_status.debug");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,sdm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),sdm.Debug.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("device_status.firmware_revision");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,sdm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),sdm.FirmwareRevision.GetValueOrDefault(-1).ToString(),string.Empty));
                    break;
                    case "WFObservationTempestMessage":
                        WFObservationTempestMessage otm = (WFObservationTempestMessage)msg;
                        seriesId = Equis.GetSeriesId("obs_st.wind_lull");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.WindLull.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.wind_avg");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.WindAvg.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.wind_gust");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.WindGust.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.wind_direction");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.WindDirection.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.wind_samp_intrvl");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.WindSampleInterval.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.pressure");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.StationPressure.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.temp");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.AirTemperature.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.lux");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.Illuminance.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.uv");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.UV.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.solar_rad");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.SolarRadiation.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.precip");
                        if(-1!=seriesId){
                            switch(otm.PercipationType){
                                case PercipationType.None:
                                    datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.RainAmountPreviosMinute.GetValueOrDefault(-1).ToString(),"NONE"));
                                    break;
                                case PercipationType.Rain:
                                    datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.RainAmountPreviosMinute.GetValueOrDefault(-1).ToString(),"Rain"));
                                    break;
                                case PercipationType.Hail:
                                    datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.RainAmountPreviosMinute.GetValueOrDefault(-1).ToString(),"Hail"));
                                    break;
                                case PercipationType.RainAndHail:
                                    datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.RainAmountPreviosMinute.GetValueOrDefault(-1).ToString(),"Mix"));
                                    break;
                            }
                        } 
                        seriesId = Equis.GetSeriesId("obs_st.strike_distance");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.LightningStrikeAvgDistance.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.strike_count");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.LightningStrikeCount.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.battery");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.Battery.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("obs_st.interval");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,otm.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),otm.ReportInterval.GetValueOrDefault(-1).ToString(),string.Empty));
                    break;
                    case "WFStatusHubMessage":
                        WFStatusHubMessage shm = (WFStatusHubMessage)msg;
                        seriesId = Equis.GetSeriesId("hub_status.uptime");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),shm.Uptime.GetValueOrDefault(TimeSpan.Zero).Seconds.ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("hub_status.firmware_revision");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),shm.Version.GetValueOrDefault(-1).ToString(),null==shm.FirmwareRevision?string.Empty:shm.FirmwareRevision));
                        seriesId = Equis.GetSeriesId("hub_status.rssi");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),shm.RSSI.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("hub_status.seq");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),shm.Seq.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("hub_status.radio_reboot");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),shm.RebootCount.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("hub_status.radio_i2c_errors");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),shm.I2CBusErrorCount.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("hub_status.radio_network_id");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),shm.RadioNetworkId.GetValueOrDefault(-1).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("hub_status.radio_status");
                        if(-1!=seriesId){
                            switch(shm.RadioStatus){
                                case RadioStatus.Off:
                                datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),"0",string.Empty));
                                break;
                                case RadioStatus.On:
                                datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),"1",string.Empty));
                                break;
                                case RadioStatus.Active:
                                datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),"3",string.Empty));
                                break;
                                case RadioStatus.BLEConnected:
                                datum.Add(new LoggerDatum((int)seriesId,shm.TimeStamp.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),"7",string.Empty));
                                break;
                            }
                        }
                    break;
                    case "WFRapidWindMessage":
                        WFRapidWindMessage rw = (WFRapidWindMessage)msg;
                        seriesId = Equis.GetSeriesId("rapid_wind.wind_speed");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,rw.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),rw.WindSpeed.GetValueOrDefault(0).ToString(),string.Empty));
                        seriesId = Equis.GetSeriesId("rapid_wind.wind_direction");
                        if(-1!=seriesId) datum.Add(new LoggerDatum((int)seriesId,rw.Time.GetValueOrDefault(DateTime.UtcNow).ToLocalTime(),rw.WindDirection.GetValueOrDefault(0).ToString(),string.Empty));

                    break;
                    default:
                        //Log the message types we don't know bout. Lightning is one of them.
                        _logger.LogInformation(mt.Name, 
                            mt.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Select(pi => $"{pi.Name}:" + (null==pi.GetValue(mt)? string.Empty : pi.GetValue(mt)?.ToString() ))
                            .ToArray());
                    break;

                }

                if(0<datum.Count && null!=Equis)
                {
                    IEnumerable<LoggerDatum> retry = Equis.PostLoggerDatum(datum).GetAwaiter().GetResult();                
                }
            })


            .ListenAsync(stoppingToken);
        }

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
