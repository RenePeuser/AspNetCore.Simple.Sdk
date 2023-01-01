using System;
using System.Linq;
using AspNetCore.Simple.Sdk.ApplicationInsight.TelemetryProcessors;
using Extensions.Pack;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApplicationInsight
{
    public static class AddApplicationInsightsSettingsExtension
    {
        public static void AddApplicationInsightsSettings(this IServiceCollection services, IConfiguration configuration)
        {
            if (services.IsAlreadyRegistered<ApplicationInsightsSettings>())
            {
                return;
            }

            if (configuration.TryGetSettings<ApplicationInsightsSettings>(out var applicationInsightsSettings))
            {
                var eventTelemetryNamesToIgnore = configuration.GetSection("ApplicationInsights:EventTelemetryFilterSettings:NamesToIgnore").Value?.Split(",").Select(value => value.Trim()).ToArray() ?? Array.Empty<string>();
                var requestTelemetryNamesToIgnore = configuration.GetSection("ApplicationInsights:RequestTelemetryFilterSettings:NamesToIgnore").Value?.Split(",").Select(value => value.Trim()).ToArray() ?? Array.Empty<string>();
                var traceTelemetryNamesToIgnore = configuration.GetSection("ApplicationInsights:TraceTelemetryFilterSettings:NamesToIgnore").Value?.Split(",").Select(value => value.Trim()).ToArray() ?? Array.Empty<string>();

                applicationInsightsSettings = applicationInsightsSettings with
                {
                    EventTelemetryFilterSettings = applicationInsightsSettings.EventTelemetryFilterSettings with { NamesToIgnore = eventTelemetryNamesToIgnore },
                    RequestTelemetryFilterSettings = applicationInsightsSettings.RequestTelemetryFilterSettings with { NamesToIgnore = requestTelemetryNamesToIgnore },
                    TraceTelemetryFilterSettings = applicationInsightsSettings.TraceTelemetryFilterSettings with { NamesToIgnore = traceTelemetryNamesToIgnore },
                };


                services.AddSingletonIfNotExists(applicationInsightsSettings);

                // We will not to inject anytime full application insights configuration
                services.AddSingletonIfNotExists(applicationInsightsSettings.EventTelemetryFilterSettings);
                services.AddSingletonIfNotExists(applicationInsightsSettings.RequestTelemetryFilterSettings);
                services.AddSingletonIfNotExists(applicationInsightsSettings.TraceTelemetryFilterSettings);
            }
        }
    }

    public record ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; init; } = "00000000000000000000000";

        public EventTelemetryFilterSettings EventTelemetryFilterSettings { get; init; } = new EventTelemetryFilterSettings();

        public RequestTelemetryFilterSettings RequestTelemetryFilterSettings { get; init; } = new RequestTelemetryFilterSettings();

        public TraceTelemetryFilterSettings TraceTelemetryFilterSettings { get; init; } = new TraceTelemetryFilterSettings();

    }

    internal static class AddApplictionInsightsExtensions
    {
        internal static void AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            // Only if configuration is available
            if (configuration.TryGetSettings<ApplicationInsightsSettings>(out _).IsFalse())
            {
                return;
            }

            // If Telemetry client already registered go out.
            if (services.IsAlreadyRegistered<TelemetryClient>())
            {
                return;
            }

            services.AddTelemetryClient();
            services.AddTelemetryProcessors(configuration);
            services.AddTelemetryInitializers();
            services.AddTelemetryLoggingBehavior();
        }

        internal static void AddTelemetryClient(this IServiceCollection services)
        {
            // If Telemetry client already registered go out.
            if (services.IsAlreadyRegistered<TelemetryClient>())
            {
                return;
            }

            services.AddApplicationInsightsTelemetry(options => options.EnableAdaptiveSampling = false);
        }

        internal static void AddTelemetryProcessors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEventTelemetryFilter(configuration);
            services.AddRequestTelemetryFilter(configuration);
            services.AddTraceTelemetryFilter(configuration);
        }

        internal static void AddTelemetryInitializers(this IServiceCollection services)
        {
            services.AddRequestBodyInitializer();
            services.AddBetterLoggingBehavior();
        }

        internal static void AddTelemetryLoggingBehavior(this IServiceCollection services)
        {
            services.AddBetterLoggingBehavior();
        }
    }
}
