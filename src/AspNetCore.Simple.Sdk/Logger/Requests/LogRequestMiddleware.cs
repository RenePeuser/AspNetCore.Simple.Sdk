using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Requests
{
    public class LogRequestMiddleware(ILogger<LogRequestMiddleware> logger,
                                      IJsonSerializer jsonSerializer) : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestHeaders = context.Request.Headers.Select(h => new { h.Key, Value = h.Value.First() }).Where(item => !item.Key.Contains("Auth")).ToList();
            var responseHeaders = context.Response.Headers.Select(h => new { h.Key, Value = h.Value.First() }).Where(item => !item.Key.Contains("Auth")).ToList();

            var logInfo = new { RequestHeaders = responseHeaders, ResponseHeaders = requestHeaders };

            var serialize = jsonSerializer.Serialize(logInfo);

            logger.LogInformation(serialize);

            return next(context);
        }
    }
}
