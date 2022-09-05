using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight.TelemetryProcessors
{
    internal static class AddTraceTelemetryFilterExtension
    {
        public static void AddTraceTelemetryFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsSettings(configuration);

            services.AddApplicationInsightsTelemetryProcessor<TraceTelemetryFilter>();
        }
    }

    public record TraceTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    internal class TraceTelemetryFilter : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _telemetryProcessor;
        private readonly TraceTelemetryFilterSettings _traceTelemetryFilterSettings;

        // next will point to the next TelemetryProcessor in the chain.
        public TraceTelemetryFilter(ITelemetryProcessor telemetryProcessor, TraceTelemetryFilterSettings traceTelemetryFilterSettings)
        {
            _telemetryProcessor = telemetryProcessor;
            _traceTelemetryFilterSettings = traceTelemetryFilterSettings;
        }

        public void Process(ITelemetry item)
        {
            if (item is TraceTelemetry traceTelemetry)
            {
                if (_traceTelemetryFilterSettings.NamesToIgnore.Any(name => traceTelemetry.Message.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (traceTelemetry.Properties.TryGetValue("RequestPath", out var requestPath))
                {
                    if (_traceTelemetryFilterSettings.NamesToIgnore.Any(name => requestPath.Contains(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return;
                    }
                }
            }

            _telemetryProcessor.Process(item);
        }
    }
}
