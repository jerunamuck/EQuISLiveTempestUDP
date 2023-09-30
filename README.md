# EQuIS Live agent for Tempest WF UDP API v1.72

## Abstract

Tempest WF weather stations broadcast live data over UDP port 50222 in JSON format. While the base dub publishes this data to Weather Flow's MQTT server, having access to the weather data in real time creates an oportunity to use that data for automation even during wide area network outages. This project collects that data and publishes it to an EarthSoft Inc. EQuIS Enterprise server hosted on site using the EQuIS Live module. Spoiler Alert; I'm a developer for EarthSoft so I have access to their entire product line. 

A secondary benifit of this project is to serve as an example of how to publish live (near real time) data to EQuIS using the REST API.

## Revision History

| date | version | description |
|------|---------|-------------|
| 2021-03-01|0.1 | Project creation |

## Dependencies

The following expectationss must be met in order to deploy this tool:
* [EQuIS Enterprise v7.23.*](https://earthsoft.com/products/enterprise/) with EQuIS Live and API license
* [Tempest WF station](https://weatherflow.com/tempest-weather-system/) in same LAN as this project is executing
* Microsoft dotnet-6.0 runtime

## Building


## Use

This tool may be executed either as a console application / daemon or as a Windows Service.

Configure EQuIS Live data loggers and series for the weather station and base station.  To do this, modify the files ./EDD/TempestLoggers.LIVE.csv and ./EDD/TempestSeries.LIVE.csv as needed and load with EQuIS Data Processor using the LIVE format. 

Modify the field Logger.SysLocCode to match the EQuIS sys_loc_code for the location of the weather station. Modify the fields Logger.LoggerCode and LoggerSeries.LoggerCode to match the serial_number values for your Tempest Weather Flow station and hub. These values can be found in your WeatherFlow app or by running EQuISLiveTempestUDP --echo and copying the values of the "serial_number" parameter from the JSON data displayed. The serial_number for the HUB can be identified by records containing "type":"hub_status".

Login to EQuIS Enterprise and create a new API Token through your user profile.

Running as console or daemon:
EQuISLiveTempestUDP.exe -l LocationCode -h https://EarthSoftServer.local/equis/ -a eW91ciBBUEkgVG9rZW4gSGVyZQ== 

## EQuIS Live Logger Configuration.

Each WeatherFlow station is paired with a network attached hub. Both of these devices have a unique serial number. These serial numbers are used to map both the station and hub to EQuIS loggers by entering the WeatherFlow device's serial number as ST_LOGGER.LOGGER_CODE.

Similarly, the EQuIS Live logger series is mapped to a UDP message property through ST_LOGGER_SERIES.SENSOR_CODE. Since we're using the NuGet package WeatherFlowUdpListener we'll be using it's class names and properties in SENSOR_CODE. The format for SENSOR_CODE is one of: ClassName.PropertyName or ClassName.PropertyName[index]. The EQuIS EDD WeatherFlow_Live.csv is provided to aid in setting up EQuIS Live Loggers and Logger Series for use with this tool.

## Theory of Operation

Upon start up, a REST call is made to retrieve all loggers and logger series associated with the LocationCode provided in the command line using EQuIS REST oData API. When a UDP message is received, WeatherFlowUdpListener parses it into a message class and calls the OnReceiveMessage event handler. The logger is identified by searching the cached colleciton of loggers for one matching SENSOR_CODE==WFMessage.SerialNumber.  For each logger series, a WMMessage property is mapped onto an ST_LOGGER_DATUM object producing a list of DATUM objects. For each DATUM object, a POST call is made to EQuIS API/oData/ST_LOGGER_DATUM making the data available in near real time.