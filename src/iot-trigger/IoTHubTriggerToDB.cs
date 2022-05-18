using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using temperature.frontend.Shared;
using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

namespace Company.Function
{
    public static class IoTHubTriggerToDB
    {
        [FunctionName("IoTHubTriggerToDB")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "IotHubEventHubString")] EventData message, ILogger log)
        {
            var deviceid = message.SystemProperties["iothub-connection-device-id"].ToString();
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)} from device:{deviceid}");

            try
            {
                var tmsg = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(message.Body.Array));
                var model = new WeatherForecast
                {
                    DeviceId = deviceid,
                    TemperatureC = tmsg.temperature,
                    Date = DateTime.Now,
                };
                if (tmsg.TemperatureC == 0.00)
                {
                    return;
                }

                var dbstring = Environment.GetEnvironmentVariable("SQLConn");
                using var con = new SqlConnection(dbstring);
                con.Open();
                if (con.State == ConnectionState.Open)
                {
                    /**
                    create table WeatherForecast(
	                    DeviceId varchar(50) not null,
	                    Date datetime not null,
	                    TemperatureC int not null
                    );
                     */
                    string strCmd = $"insert into dbo.WeatherForecast(TemperatureC,Date,DeviceId) values ({model.TemperatureC},'{model.Date}','{model.DeviceId}' )";

                    var sqlcmd = new SqlCommand(strCmd, con);
                    int n = sqlcmd.ExecuteNonQuery();
                    if (n > 0)
                    {
                        log.LogInformation("Save to db successfully");
                    }
                    else
                    {
                        log.LogError("Save to db error");
                    }
                }
                con.Close();
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }
    }
}