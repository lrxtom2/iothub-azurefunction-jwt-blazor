using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using temperature.frontend.Shared;

namespace func_openapi
{
    public class WeatherForecastApi
    {
        private readonly ILogger<WeatherForecastApi> _logger;

        public WeatherForecastApi(ILogger<WeatherForecastApi> log)
        {
            _logger = log;
        }

        [Authorize]
        [FunctionName("GetWeatherForecast")]
        [OpenApiOperation(operationId: "WeatherForecast_Get", tags: new[] { "WeatherForecast" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "WeatherForecast")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var dbstring = Environment.GetEnvironmentVariable("SQLConn");
            using var con = new SqlConnection(dbstring);
            con.Open();
            var list = new List<WeatherForecast>();
            if (con.State == ConnectionState.Open)
            {
                var strCmd = $"select * from WeatherForecast";

                var command = new SqlCommand(strCmd, con);
                var dr = command.ExecuteReader();
                while (dr.Read())
                {
                    var model = new WeatherForecast();
                    model.TemperatureC = int.Parse(dr["TemperatureC"].ToString());
                    model.Date = DateTime.Parse(dr["Date"].ToString());
                    model.DeviceId = dr["DeviceId"].ToString();
                    list.Add(model);
                }
            }
            con.Close();

            return new JsonResult(list);
        }
    }
}