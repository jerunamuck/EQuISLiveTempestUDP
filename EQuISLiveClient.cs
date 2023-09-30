using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EQuISLiveTempestUDP
{
    public class EQuISLiveClient
    {
        private Uri _uri = new Uri("http://localhost");
        private string _authToken = string.Empty; 
        
        private string _locationCode = string.Empty;
        public List<Logger>? Loggers;

        private IConfiguration _config;
        private readonly ILogger<EQuISLiveClient> _log;

        private int _httpTimeOut = 240;//Net.Core default.

        public EQuISLiveClient(ILogger<EQuISLiveClient> logger, IConfiguration config){
            _log = logger;
            _config = config;
        }


        public async Task<bool> Init()
        {
            this._uri = new Uri(_config.GetSection("EQuISLiveClient").GetValue<string>("BaseUri"));
            this._authToken = _config.GetSection("EQuISLiveClient").GetValue<string>("Authentication");
            this._locationCode = _config.GetSection("EQuISLiveClient").GetValue<string>("LocationCode");
            string val = _config.GetSection("EQuISLiveClient").GetValue<string>("HttpTimeOutSeconds");
            _log.LogInformation("Using EQuIS at {Uri}",this._uri);
            if(int.TryParse(val,out int i)) this._httpTimeOut=i;
            CheckInitialization();
            this.Loggers = await this.GetLoggersAsync(_locationCode);
            if(null==this.Loggers || this.Loggers.Count==0)
            {return false;}
            else
            {return true;}
        }

        public int GetSeriesId(string seriesName)
        {
            int seriesId = -1;
            foreach(Logger l in this.Loggers)
            {
                foreach(LoggerSeries s in l.series)
                {
                    if(s.SERIES_NAME.Equals(seriesName,StringComparison.CurrentCultureIgnoreCase))
                    {
                        seriesId = s.LOGGER_SERIES_ID;
                        break;
                    }
                }
                if(seriesId != -1){ break; }
            }
            
            return seriesId;
        }

        internal void CheckInitialization()
        {
            if(this._uri.ToString().Equals("http://localhost") )
            {
                throw new InvalidOperationException("Null base uri? where is the EQuIS API?");
            }
            else if( !this._uri.ToString().EndsWith("/api",StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException($"{this._uri} must end with /api ");
            }
        }

        ///
        /// Retrieve EQuIS Loggers for sys_loc_code
        internal async Task<List<Logger>?> GetLoggersAsync(string locationCode)
        {
            CheckInitialization();
            List<Logger>? loggers = null;
            Uri eq ;
            if(string.IsNullOrEmpty(locationCode))
            {            
                eq = new Uri(this._uri.ToString().Replace("/api","/api/odata/DT_LOGGER",StringComparison.CurrentCultureIgnoreCase));
                }
            else
            {
                eq = new Uri(this._uri.ToString().Replace("/api",$"/api/odata/DT_LOGGER?$filter=SYS_LOC_CODE eq '{locationCode}'",StringComparison.CurrentCultureIgnoreCase));
            }
            using(HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(TimeSpan.TicksPerSecond * this._httpTimeOut);
                using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get,eq))
                {
                    if(!string.IsNullOrEmpty(this._authToken))
                    {
                        AuthenticationHeaderValue auth;
                        string[] parts = this._authToken.Split(new char[] {' '});
                        if(parts.Length==2)
                            auth = new AuthenticationHeaderValue(parts[0],parts[1]);
                        else
                            auth = new AuthenticationHeaderValue("barer",this._authToken);

                        req.Headers.Authorization = auth;
                    }
                    using (HttpResponseMessage response = await client.SendAsync(req))
                    {
                        if(response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            oDataLogger content = await JsonSerializer.DeserializeAsync<oDataLogger>(await response.Content.ReadAsStreamAsync());
                            if(null != content?.value && content.value.Length>0)
                            loggers = new List<Logger>(content.value);
                            _log.LogInformation("Fount {count} loggers for SYS_LOC_CODE {locationCode}",content.value.Length, locationCode);
                        }
                        else
                        {
                                string msg = await response.Content.ReadAsStringAsync();
                                if(string.IsNullOrEmpty(msg))
                                    _log.LogError("GET {uri} failed {status_code}",eq, response.StatusCode);
                                else
                                    _log.LogError("GET {uri} failed {status_code}: {msg}",eq, response.StatusCode,msg);
                        }
                    }
                }
            }

            if(null != loggers)
            {
                foreach(Logger l in loggers)
                {
                    l.series = await GetLoggerSeriesAsync(l.LOGGER_ID);
                }
            }

            return loggers;
        }

        internal async Task<List<LoggerSeries>?> GetLoggerSeriesAsync(long logger_id)
        {
            CheckInitialization();
            List<LoggerSeries>? series =null;
            Uri eq ;

            eq = new Uri(this._uri.ToString().Replace("/api",$"/api/odata/DT_LOGGER_SERIES?$filter=LOGGER_ID eq {logger_id}",StringComparison.CurrentCultureIgnoreCase));
            using(HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(TimeSpan.TicksPerSecond * this._httpTimeOut);
                using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get,eq))
                {
                    if(!string.IsNullOrEmpty(this._authToken))
                    {
                        AuthenticationHeaderValue auth;
                        string[] parts = this._authToken.Split(new char[] {' '});
                        if(parts.Length==2)
                            auth = new AuthenticationHeaderValue(parts[0],parts[1]);
                        else
                            auth = new AuthenticationHeaderValue("barer",this._authToken);

                        req.Headers.Authorization = auth;
                    }
                    using (HttpResponseMessage response = await client.SendAsync(req))
                    {
                        if(response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            oDataLoggerSeries content = await JsonSerializer.DeserializeAsync<oDataLoggerSeries>(await response.Content.ReadAsStreamAsync());
                            if(null!=content?.value && content.value.Length>0)
                                series = new List<LoggerSeries>(content.value);
                                _log.LogInformation("Found {count} series for LOGGER_ID {loggerId}",content.value.Length, logger_id);
                        }
                        else
                        {
                                string msg = await response.Content.ReadAsStringAsync();
                                if(string.IsNullOrEmpty(msg))
                                    _log.LogError("GET {uri} failed {status_code}",eq, response.StatusCode);
                                else
                                    _log.LogError("GET {uri} failed {status_code}: {msg}",eq, response.StatusCode,msg);
                        }
                    }
                }
            }
            return series;
        }
        /// <summary>
        ///     Send the logger datum items via OData call.
        /// </summary>
        /// <param name="datum"></param>
        /// <returns> List of datum values that were unsuccessful so we can retry sending them. </returns>
        public async Task<IEnumerable<LoggerDatum>> PostLoggerDatum(IEnumerable<LoggerDatum> datum)
        {
            /*
                {
                    "LOGGER_SERIES_ID":352,
                    "DATUM_UTC_DT":"2023-08-14T17:25:00",
                    "DATUM_VALUE":3.14,
                    "DATUM_QUALIFIER":"ABCD"
                }            
            */
            List<LoggerDatum> retry = new List<LoggerDatum>();

            using(HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(TimeSpan.TicksPerSecond * this._httpTimeOut);
                Uri eq ;
                eq = new Uri(this._uri.ToString().Replace("/api",$"/api/odata/DT_LOGGER_DATUM",StringComparison.CurrentCultureIgnoreCase));
                //Unfortunately we need to send them one at a time.
                foreach(LoggerDatum d in datum)
                {
                    using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post,eq))
                    {
                        if(!string.IsNullOrEmpty(this._authToken))
                        {
                            AuthenticationHeaderValue auth;
                            string[] parts = this._authToken.Split(new char[] {' '});
                            if(parts.Length==2)
                                auth = new AuthenticationHeaderValue(parts[0],parts[1]);
                            else
                                auth = new AuthenticationHeaderValue("barer",this._authToken);

                            req.Headers.Authorization = auth;
                        }                        
                        req.Content = new StringContent(JsonSerializer.Serialize(d),Encoding.UTF8,"application/json");
                        using (HttpResponseMessage response = await client.SendAsync(req))
                        {
                            // Just retry the ones that fail.
                            if(!(response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK))
                            {
                                retry.Add(d);
                                string msg = await response.Content.ReadAsStringAsync();
                                if(string.IsNullOrEmpty(msg))
                                    _log.LogError("POST {uri} with {datum} failed {status_code}",eq,d, response.StatusCode);
                                else
                                    _log.LogError("POST {uri} with {datum} failed {status_code}: {msg}",eq,d, response.StatusCode,msg);
                            }
                        }
                    }
                }
                if(retry.Count>0){
                    int success = datum.Count() - retry.Count();
                    _log.LogDebug("Posted {success} DATUM, will retry {fails}", success ,retry.Count());
                }
                else
                    _log.LogDebug("Posted {success} DATUM", datum.Count());

            }
            return retry;
        }

        public static LoggerDatum NewLoggerDatum(int seriesId, DateTime dateTime, string value, string qualifier){
            var val = new LoggerDatum() {LOGGER_SERIES_ID=seriesId, DATUM_UTC_DT=dateTime, DATUM_VALUE=value, DATUM_QUALIFIER = qualifier};
            if (val.DATUM_UTC_DT==DateTime.MinValue){val.DATUM_UTC_DT=DateTime.UtcNow;}
            if(!string.IsNullOrEmpty(qualifier) && qualifier.Length>4)
            {
                val.DATUM_QUALIFIER = qualifier.Substring(0,4);
            }
            return val;
        }




    }

    public class LoggerDatum
    {
        public LoggerDatum()
        {
            this.DATUM_UTC_DT = DateTime.UtcNow;
        }

        public LoggerDatum (long seriesId, DateTime dateTime, string value, string qualifier)
        {
            this.LOGGER_SERIES_ID=seriesId;
            this.DATUM_UTC_DT=dateTime;
            this.DATUM_VALUE=value;
            if(!string.IsNullOrEmpty(qualifier))
            {
                if(qualifier.Length>4)
                    this.DATUM_QUALIFIER = qualifier.Substring(0,4);
                else
                    this.DATUM_QUALIFIER = qualifier;
            }
        }
        public long LOGGER_SERIES_ID {get; set;}
        public DateTime DATUM_UTC_DT {get; set;}
        public string? DATUM_VALUE {get; set;}
        public string? DATUM_QUALIFIER {get; set;}
    }


    [Serializable]
    public class oDataLogger
    {
        [JsonPropertyName("@odata.context")]
        public string oDataContext {get;set;}
        public Logger[] value {get;set;}

        [JsonPropertyName("@odata.id")]
        public string oDataId {get;set;}

        [JsonPropertyName("@odata.editLink")]
        public string oDataEditLink {get;set;}

    }

    [Serializable]
    public class oDataLoggerSeries
    {
        [JsonPropertyName("@odata.context")]
        public string oDataContext {get;set;}
        public LoggerSeries[] value {get;set;}

        [JsonPropertyName("@odata.id")]
        public string oDataId {get;set;}

        [JsonPropertyName("@odata.editLink")]
        public string oDataEditLink {get;set;}
    }

    [Serializable]
    
    public class Logger
    {
        public long LOGGER_ID{get;set;}
        public string LOGGER_CODE {get;set;}
        public string LOGGER_DESC {get;set;}
        public DateTime? START_DATE {get;set;}
        public DateTime? END_DATE {get;set;}
        public float? UTC_OFFSET_HRS {get;set;}
        public string LIVE_DATE_SOURCE {get;set;}
        public string REMARK {get;set;}
        public long? FACILITY_ID{get;set;}
        public string SYS_LOC_CODE{get;set; }
        public string DEVICE_CODE{get;set;}
        public string DEVICE_DESC{get;set;}
        public long? LOG_PERIOD_S{get;set;}
        public long? TRANSMIT_PERIOD_S{get;set;}
        public long? SYMBOL_ID{get;set;}
        public string CUSTOM_FIELD_1 {get;set;}
        public string CUSTOM_FIELD_2 {get;set;}
        public string CUSTOM_FIELD_3 {get;set;}
        public string CUSTOM_FIELD_4 {get;set;}
        public string CUSTOM_FIELD_5 {get;set;}
        public string STATUS_FLAG {get;set;}
        public long? DATA_EXPIRATION_DAYS {get;set;}
        public string SENSOR_CODE {get;set;}
        public List<LoggerSeries> series{get;set;}
    }



    [Serializable]
    public class LoggerSeries
    {
            public int LOGGER_ID {get; set;}
            public int LOGGER_SERIES_ID {get; set;}
            public string SERIES_NAME {get; set;}
            public string SERIES_DESC {get; set;}
            public string SERIES_UNIT {get; set;}
            public string SERIES_FUNCTION {get; set;}
            public int? DERIVATION {get; set;}
            public int? LOG_PERIOD_S {get; set;}
            public string LOCATION_PARAM_CODE {get; set;}
            public int? SERIES_ORDINAL {get; set;}
            public string SENSOR_CODE {get; set;}
            public string SENSOR_NAME {get; set;}
            public string SENSOR_RANGE {get; set;}
            public string SENSOR_OFFSET {get; set;}
            public string DEFAULT_DATUM_QUALIFIER {get; set;}
            public int? FACILITY_ID {get; set;}
            public string SYS_LOC_CODE {get; set;}
            public int? SYMBOL_ID {get; set;}
            public float? TYPICAL_MAX_VALUE {get; set;}
            public float? TYPICAL_MIN_VALUE {get; set;}
            public string CUSTOM_FIELD_1 {get; set;}
            public string CUSTOM_FIELD_2 {get; set;}
            public string CUSTOM_FIELD_3 {get; set;}
            public string CUSTOM_FIELD_4 {get; set;}
            public string CUSTOM_FIELD_5 {get; set;}
            public string REMARK {get; set;}
            public string STATUS_CODE {get; set;}
            public string SERIES_FUNCTION_INFO {get; set;}
            public float? DEPTH {get; set;}
            public string DEPTH_UNIT {get; set;}
            public string series_type {get; set;}
            public int? DATA_EXPIRATION_DAYS {get; set;}
            public int? EBATCH {get; set;}
            public int? EUID {get; set;}
    }
}