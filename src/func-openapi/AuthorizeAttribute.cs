using func_openapi.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace func_openapi
{
    public class AuthorizeAttribute : FunctionInvocationFilterAttribute
    {
        public AuthorizeAttribute()
        {
        }

        public override Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            return base.OnExecutedAsync(executedContext, cancellationToken);
        }

        public override async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            var req = executingContext.Arguments.First().Value as HttpRequest;
            string authorization = req.Headers["Authorization"];

            var auth = new JWTValidateHelper(authorization);
            if (!auth.IsValid)
            {
                var message = "unauthencation";
                var response = req.HttpContext.Response;
                response.ContentType = "text/plain";
                response.ContentLength = message.Length;
                response.StatusCode = StatusCodes.Status404NotFound;
                await response.WriteAsync(message, cancellationToken: cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
            await base.OnExecutingAsync(executingContext, cancellationToken);
        }
    }
}