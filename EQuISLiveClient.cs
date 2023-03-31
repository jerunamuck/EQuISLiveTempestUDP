using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EQuISLiveTempestUDP
{
    public class EQuISLiveClient
    {
        private Uri _uri;
        private string authToken; 
        
        public EQuISLiveClient(){}

        public EQuISLiveClient(Uri baseUri, string authentication)
        {
            this._uri = baseUri;
            this.authToken = authentication;
        }

        public List<Logger> GetLoggers(string locationCode)
        {}

    }

    [Serializable]
    public class Logger
    {
        public long logger_id{get;set;}
        public string logger_code{get;set;}
        public string dogger_desc{get;set;}
        public long? start_date{get;set;}
        public long? and_date{get;set;}
        public float? utc_offset_hrs{get;set;}
        public string live_data_source{get;set;}
        public string remark{get;set;}
        public long? facility_id{get;set;}
        public string sys_loc_code{get;set; }
        public string device_code{get;set;}
        public string device_desc{get;set;}
        public long? log_period_s{get;set;}
        public long? transmit_period_s{get;set;}
        public long? symbol_id{get;set;}
        public string custom_field1_{get;set;}
        public string custom_field2_{get;set;}
        public string custom_field3_{get;set;}
        public string custom_field4_{get;set;}
        public string custom_field5_{get;set;}
        public string status_flag{get;set;}
        public long? data_expiration_days{get;set;}
        public string sensor_code{get;set;}
        public List<Series> series{get;set;}
    }

        [Serializable]
        public class Series
        {}
}