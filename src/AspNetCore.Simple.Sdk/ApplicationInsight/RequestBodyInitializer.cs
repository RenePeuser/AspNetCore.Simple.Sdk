using System.Net.Http;
using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight
{
    public static class AddRequestBodyInitializerExtension
    {
        public static void AddRequestBodyInitializer(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ITelemetryInitializer, RequestBodyInitializer>();
        }
    }

    public class RequestBodyInitializer : ITelemetryInitializer
    {
        private const string RequestKeyName = "RequestBody";
        private const string ResponseKeyName = "ResponseBody";

        public void Initialize(ITelemetry telemetry)
        {
            // Ignore everything except DependencyTelemetry
            if (telemetry is not DependencyTelemetry dependencyTelemetry)
            {
                return;
            }

            dependencyTelemetry.TryGetOperationDetail("HttpResponse", out var rawResponse);

            // Each dependency will called twice, once for the request and once for response, but on response is also the request information available
            // Ignore every successful Response, that logging should just be done for tracking issues
            // TODO: Ignore in local debug mode (#37682)
            if (rawResponse is not HttpResponseMessage responseMessage || responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            dependencyTelemetry.TryGetOperationDetail("HttpRequest", out var rawRequest);
            var requestMessage = rawRequest as HttpRequestMessage;
            if (requestMessage?.RequestUri == null || requestMessage.RequestUri.AbsoluteUri.Contains("oauth"))
            {
                return;
            }

            if ((requestMessage.Method == HttpMethod.Post || requestMessage.Method == HttpMethod.Put) && requestMessage.Content != null)
            {
                var contentLength = requestMessage.Content.Headers.ContentLength;
                if (contentLength <= 10240)
                {
                    var requestBody = requestMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    dependencyTelemetry.Properties.Add(RequestKeyName, requestBody);
                }
            }

            var responseContentLength = responseMessage.Content.Headers.ContentLength;
            if (responseContentLength <= 10240)
            {
                var responseBody = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dependencyTelemetry.Properties.Add(ResponseKeyName, responseBody);
            }
        }
    }
}
