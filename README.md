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
dotnet run EQuISLiveTempestUDP.dll

## EQuIS Live Logger Configuration.

Modify appsettings.json and provide the following configuration fields.

  "EQuISLiveClient":{
    "BaseUri":"http://localhost/path/api",
    "Authentication":"Bearer eyJhbGciOiJIU---Tvai72Gg6oj4DyA2DWo8==" ,
    "LocationCode":"ST-00094830",
    "HttpTimeOutSeconds": 240
  }

where 
  BaseUri is the URL of an EarthSoft EQuIS Enterprise instance. The Uri must end with '/api' or a configuration error will be thrown.
  Authentication is a bearer token issued by that EQuIS Enterprise instance. This requires a REST API license with both API.Live and API.OData controllers installed. Remember that OData controller requires that the user be granted membership in an ALS (Application Level Security) Role with write permission on DT_LOGGER_DATUM.
  LocationCode is the unique SYS_LOCE_CODE for the loggers and logger series to be used with this weather station. While EQuIS only requires uniqueness withing a facility, this application requires the SYS_LOC_CODE be unique within the database. We recommend using the Tempest WX Serial Number.

## Theory of Operation

Upon start up, a REST call is made to retrieve all loggers and logger series associated with the LocationCode provided in the command line using EQuIS REST oData API. When a UDP message is received from the Hum, the payload is parsed using NuGet package WeatherFlowUdpListener. Individual data points are forwarded to EQuIS through it's OData REST API controller.

