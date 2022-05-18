using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace func_openapi
{
    public class UserModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class Auth
    {
        private readonly ILogger<Auth> _logger;

        public Auth(ILogger<Auth> log)
        {
            _logger = log;
        }

        [FunctionName("Token")]
        [OpenApiOperation(operationId: "Auth_Login", tags: new[] { "Auth" })]
        [OpenApiRequestBody("application/json", typeof(UserModel),
            Description = "JSON request body containing { UserName, Password}")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string),
            Description = "The OK response message containing a JSON result.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/token")] HttpRequest req)
        {
            _logger.LogInformation("Openapi auth.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string userName = data?.username;

            if (string.IsNullOrEmpty(userName) || userName != "admin")
            {
                return new UnauthorizedResult();
            }

            var tokenResult = new
            {
                AccessToken = IssuingJWT(userName),
            };
            return new OkObjectResult(JsonConvert.SerializeObject(tokenResult));
        }

        private string IssuingJWT(string user)
        {
            var token = JwtBuilder.Create()
                      .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                      .WithSecret("authkey1233456767890")
                      .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                      .AddClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "pwcfuncuser")
                      .AddClaim("username", user)
                      .AddClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "administrator")
                      .Encode();

            return token;
        }
    }
}