using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight
{
    internal static class AddDependencyTelemetryFilterExtension
    {
        public static void AddTelemetryFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsSettings(configuration);

            services.AddApplicationInsightsTelemetryProcessor<TelemetryFilter>();
        }
    }

    public record DependencyTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    public record RequestTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    public record TraceTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    public record EventTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }



    internal sealed class TelemetryFilter : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _telemetryProcessor;
        private readonly DependencyTelemetryFilterSettings _dependencyTelemetryFilterSettings;
        private readonly EventTelemetryFilterSettings _eventTelemetryFilterSettings;
        private readonly RequestTelemetryFilterSettings _requestTelemetryFilterSettings;
        private readonly TraceTelemetryFilterSettings _traceTelemetryFilterSettings;

        // next will point to the next TelemetryProcessor in the chain.
        public TelemetryFilter(ITelemetryProcessor telemetryProcessor,
                               DependencyTelemetryFilterSettings dependencyTelemetryFilterSettings,
                               EventTelemetryFilterSettings eventTelemetryFilterSettings,
                               RequestTelemetryFilterSettings requestTelemetryFilterSettings,
                               TraceTelemetryFilterSettings traceTelemetryFilterSettings)
        {
            _telemetryProcessor = telemetryProcessor;
            _dependencyTelemetryFilterSettings = dependencyTelemetryFilterSettings;
            _eventTelemetryFilterSettings = eventTelemetryFilterSettings;
            _requestTelemetryFilterSettings = requestTelemetryFilterSettings;
            _traceTelemetryFilterSettings = traceTelemetryFilterSettings;
        }

        public void Process(ITelemetry item)
        {
            if (item is DependencyTelemetry dependencyTelemetry)
            {
                if (_dependencyTelemetryFilterSettings.NamesToIgnore.Any(name => dependencyTelemetry.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            if (item is EventTelemetry eventTelemetry)
            {
                if (_eventTelemetryFilterSettings.NamesToIgnore.Any(name => eventTelemetry.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            if (item is RequestTelemetry requestTelemetry)
            {
                if (_requestTelemetryFilterSettings.NamesToIgnore.Any(name => requestTelemetry.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

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
