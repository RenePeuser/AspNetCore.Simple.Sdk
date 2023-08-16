using System;
using System.Linq;
using AspNetCore.Simple.Sdk.ApplicationInsight.TelemetryProcessors;
using AspNetCore.Simple.Sdk.ErrorHandling;
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
                var eventTelemetryNamesToIgnore = configuration.GetSection($"{nameof(ApplicationInsightsSettings)}:{nameof(ApplicationInsightsSettings.EventTelemetryFilterSettings)}:{nameof(ApplicationInsightsSettings.EventTelemetryFilterSettings.NamesToIgnore)}").Value?.Split(",").Select(value => value.Trim()).ToArray() ?? Array.Empty<string>();
                var requestTelemetryNamesToIgnore = configuration.GetSection($"{nameof(ApplicationInsightsSettings)}:{nameof(ApplicationInsightsSettings.RequestTelemetryFilterSettings)}:{nameof(ApplicationInsightsSettings.RequestTelemetryFilterSettings.NamesToIgnore)}").Value?.Split(",").Select(value => value.Trim()).ToArray() ?? Array.Empty<string>();
                var traceTelemetryNamesToIgnore = configuration.GetSection($"{nameof(ApplicationInsightsSettings)}:{nameof(ApplicationInsightsSettings.TraceTelemetryFilterSettings)}:{nameof(ApplicationInsightsSettings.TraceTelemetryFilterSettings.NamesToIgnore)}").Value?.Split(",").Select(value => value.Trim()).ToArray() ?? Array.Empty<string>();
                var dependencyTelemetryNamesToIgnore = configuration.GetSection($"{nameof(ApplicationInsightsSettings)}:{nameof(ApplicationInsightsSettings.DependencyTelemetryFilterSettings)}:{nameof(ApplicationInsightsSettings.DependencyTelemetryFilterSettings.NamesToIgnore)}").Value?.Split(",").Select(value => value.Trim()).ToArray() ?? Array.Empty<string>();

                applicationInsightsSettings = applicationInsightsSettings with
                {
                    EventTelemetryFilterSettings = applicationInsightsSettings.EventTelemetryFilterSettings with { NamesToIgnore = eventTelemetryNamesToIgnore },
                    RequestTelemetryFilterSettings = applicationInsightsSettings.RequestTelemetryFilterSettings with { NamesToIgnore = requestTelemetryNamesToIgnore },
                    TraceTelemetryFilterSettings = applicationInsightsSettings.TraceTelemetryFilterSettings with { NamesToIgnore = traceTelemetryNamesToIgnore },
                    DependencyTelemetryFilterSettings = applicationInsightsSettings.DependencyTelemetryFilterSettings with { NamesToIgnore = dependencyTelemetryNamesToIgnore },
                };


                services.AddSingletonIfNotExists(applicationInsightsSettings);

                // We will not to inject anytime full application insights configuration
                services.AddSingletonIfNotExists(applicationInsightsSettings.EventTelemetryFilterSettings);
                services.AddSingletonIfNotExists(applicationInsightsSettings.RequestTelemetryFilterSettings);
                services.AddSingletonIfNotExists(applicationInsightsSettings.TraceTelemetryFilterSettings);
                services.AddSingletonIfNotExists(applicationInsightsSettings.DependencyTelemetryFilterSettings);
            }
            else
            {
                services.AddSingletonIfNotExists(new ApplicationInsightsSettings());
            }
        }
    }

    public record ApplicationInsightsSettings
    {
        public string ConnectionString { get; init; } = "InstrumentationKey=00000000-0000-0000-0000-000000000000;";

        public EventTelemetryFilterSettings EventTelemetryFilterSettings { get; init; } = new EventTelemetryFilterSettings();

        public RequestTelemetryFilterSettings RequestTelemetryFilterSettings { get; init; } = new RequestTelemetryFilterSettings();

        public TraceTelemetryFilterSettings TraceTelemetryFilterSettings { get; init; } = new TraceTelemetryFilterSettings();

        public DependencyTelemetryFilterSettings DependencyTelemetryFilterSettings { get; init; } = new DependencyTelemetryFilterSettings();
    }

    internal static class AddApplictionInsightsExtensions
    {
        internal static void AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure AddApplicationInsightsSettings
            services.AddApplicationInsightsSettings(configuration);

            // Only if configuration is available then we continue here
            if (configuration.TryGetSettings<ApplicationInsightsSettings>(out _).IsFalse())
            {
                return;
            }

            // If Telemetry client already registered go out.
            if (services.IsAlreadyRegistered<TelemetryClient>())
            {
                return;
            }

            services.AddTelemetryClient(configuration);
            services.AddTelemetryProcessors(configuration);
            services.AddTelemetryInitializers(configuration);
            services.AddTelemetryLoggingBehavior(configuration);
        }

        internal static void AddTelemetryClient(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.TryGetSettings<ApplicationInsightsSettings>(out var settings).IsFalse())
            {
                settings = new ApplicationInsightsSettings();
            }

            // If Telemetry client already registered go out.
            if (services.IsAlreadyRegistered<TelemetryClient>())
            {
                return;
            }

            if (settings.ConnectionString.IsNullOrWhiteSpace())
            {
                throw new ProblemDetailsException("Missing connection string for ApplicationInsights",
                                                  "Please configure you application insights settings correctly and define the connection string as well",
                                                  ("Sample", new ApplicationInsightsSettings().ToJson()));
            }

            services.AddApplicationInsightsTelemetry(options =>
            {
                options.EnableAdaptiveSampling = false;
                options.ConnectionString = settings.ConnectionString;
            });
        }

        internal static void AddTelemetryProcessors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEventTelemetryFilter(configuration);
            services.AddRequestTelemetryFilter(configuration);
            services.AddTraceTelemetryFilter(configuration);
            services.AddDependencyTelemetryFilter(configuration);
        }

        internal static void AddTelemetryInitializers(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRequestBodyInitializer();
            services.AddBetterLoggingBehavior(configuration);
        }

        internal static void AddTelemetryLoggingBehavior(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBetterLoggingBehavior(configuration);
        }
    }
}
