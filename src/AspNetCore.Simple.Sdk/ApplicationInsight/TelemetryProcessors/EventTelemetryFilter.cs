using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight.TelemetryProcessors
{
    internal static class AddEventTelemetryFilterExtension
    {
        public static void AddEventTelemetryFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsSettings(configuration);

            services.AddApplicationInsightsTelemetryProcessor<EventTelemetryFilter>();
        }
    }

    public record EventTelemetryFilterSettings
    {
        public string[] NamesToIgnore { get; init; } = Array.Empty<string>();
    }

    internal class EventTelemetryFilter : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _telemetryProcessor;
        private readonly EventTelemetryFilterSettings _eventTelemetrySettings;

        // next will point to the next TelemetryProcessor in the chain.
        public EventTelemetryFilter(ITelemetryProcessor telemetryProcessor, EventTelemetryFilterSettings eventTelemetrySettings)
        {
            _telemetryProcessor = telemetryProcessor;
            _eventTelemetrySettings = eventTelemetrySettings;
        }

        public void Process(ITelemetry item)
        {
            if (item is EventTelemetry eventTelemetry)
            {
                if (_eventTelemetrySettings.NamesToIgnore.Any(name => eventTelemetry.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            _telemetryProcessor.Process(item);
        }
    }
}
