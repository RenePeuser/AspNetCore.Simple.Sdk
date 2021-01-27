using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Requests
{
    public class LogRequestMiddleware : IMiddleware
    {
        private readonly ILogger<LogRequestMiddleware> _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public LogRequestMiddleware(ILogger<LogRequestMiddleware> logger, IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestHeaders = context.Request.Headers.Select(h => new {h.Key, Value = h.Value.First()}).Where(item => !item.Key.Contains("Auth")).ToList();
            var responseHeaders = context.Response.Headers.Select(h => new {h.Key, Value = h.Value.First()}).Where(item => !item.Key.Contains("Auth")).ToList();

            var logInfo = new {RequestHeaders = responseHeaders, ResponseHeaders = requestHeaders};

            _logger.LogInformation(_jsonSerializer.Serialize(logInfo));

            return next(context);
        }
    }
}
