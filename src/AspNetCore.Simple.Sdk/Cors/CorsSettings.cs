using System.Collections.Immutable;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Cors
{
    /// <summary>
    /// The CORS settings class to handle the CORS configurations
    /// </summary>
    public record CorsSettings
    {
        /// <summary>
        /// Provides a list of allowed origins sources
        /// </summary>
        public IImmutableList<string> Origins { get; init; } = "*".AsImmutableList();

        /// <summary>
        /// Provides the list of allowed CORS headers
        /// </summary>
        public IImmutableList<string> Headers { get; init; } = ImmutableList.Create("Origin, X-Requested-With, Content-Type, Accept");

        /// <summary>
        /// Provides if CORS credentials are allowed
        /// </summary>
        public bool AllowCredentials { get; init; } = true;
    }

    public static class AddCorsConfigurationExtension
    {
        private const string CorsSettingsName = "cors";

        public static void AddCorsSettings(this IServiceCollection services, IConfiguration configuration)
        {
            if (services.IsAlreadyRegistered<CorsSettings>())
            {
                return;
            }

            var corsSettings = GetSettingsOrDefault(configuration);

            services.AddSingletonIfNotExists(corsSettings);

            services.AddCors(o => o.AddPolicy(CorsSettingsName,
                builder =>
                {
                    builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                    builder.WithOrigins(corsSettings.Origins.ToArray());
                    builder.Build();
                }
            ));
        }

        private static CorsSettings GetSettingsOrDefault(IConfiguration configuration)
        {
            var corsSection = configuration.GetSection(nameof(CorsSettings));
            if (corsSection.IsNull())
            {
                return new CorsSettings();
            }

            var corsSettings = configuration.Get<CorsSettings>();
            if (corsSettings.IsNull())
            {
                throw new ProblemDetailsException($"Was not able get CORS settings",
                                                  $"The type: '{nameof(CorsSettings)}' could not be fetched from configuration");
            }


            // Origins
            var originsValue = corsSection[nameof(CorsSettings.Origins)];
            if (originsValue.IsNotNullOrWhiteSpace())
            {
                corsSettings = corsSettings with { Origins = originsValue.Split(",").Distinct().ToImmutableList() };
            }

            var headersValue = corsSection[nameof(CorsSettings.Headers)];
            if (headersValue.IsNotNull())
            {
                corsSettings = corsSettings with { Headers = headersValue.Split(",").Distinct().ToImmutableList() };
            }

            return corsSettings;
        }

        public static void UseCorsConfiguration(this IApplicationBuilder app)
        {
            app.UseCors(CorsSettingsName);
        }
    }
}
