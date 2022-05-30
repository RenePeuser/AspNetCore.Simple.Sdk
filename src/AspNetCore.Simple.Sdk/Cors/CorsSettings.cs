using System.Collections.Immutable;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
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
        public IImmutableList<string> Origins { get; init; } = ImmutableList<string>.Empty;

        /// <summary>
        /// Provides the list of allowed CORS headers
        /// </summary>
        public IImmutableList<string> Headers { get; init; } = ImmutableList<string>.Empty;

        /// <summary>
        /// Provides if CORS credentials are allowed
        /// </summary>
        public bool AllowCredentials { get; init; }
    }

    public static class AddCorsConfigurationExtension
    {
        private const string CorsSettingsName = "cors";

        public static void AddCorsSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var corsSettings = GetSettingsOrThrowMissingException(configuration);

            services.AddSingleton(corsSettings);

            services.AddCors(o => o.AddPolicy(CorsSettingsName,
                builder =>
                {
                    builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                    builder.WithOrigins(corsSettings.Origins.ToArray());
                    builder.Build();
                }
            ));
        }

        private static CorsSettings GetSettingsOrThrowMissingException(IConfiguration configuration)
        {
            var corsSection = configuration.GetSection(nameof(CorsSettings));
            var originsValue = corsSection[nameof(CorsSettings.Origins)];
            if (originsValue is null)
            {
                throw new ProblemDetailsException("Missing app settings",
                                                  $"The settings for {nameof(CorsSettings)}__{nameof(CorsSettings.Origins)} is missing. Please check your appsettings.json or your environment variables",
                                                  ("AppSettings", $"\"{nameof(CorsSettings)}\": {{\n    \"{nameof(CorsSettings.Origins)}\": \"*\",\n}}"),
                                                  ("Environmentvariable", $"{nameof(CorsSettings)}__{nameof(CorsSettings.Origins)}"));
            }


            var originsAsArray = originsValue.Split(",").Distinct().ToImmutableList();

            var headersValue = corsSection[nameof(CorsSettings.Headers)];
            if (headersValue is null)
            {
                throw new ProblemDetailsException("Missing app settings",
                    $"The settings for {nameof(CorsSettings)}__{nameof(CorsSettings.Headers)} is missing. Please check your appsettings.json or your environment variables",
                    ("AppSettings", $"\"{nameof(CorsSettings)}\": {{\n    \"{nameof(CorsSettings.Headers)}\": \"*\",\n}}"),
                    ("Environmentvariable", $"{nameof(CorsSettings)}__{nameof(CorsSettings.Headers)}"));
            }

            var headersAsArray = headersValue.Split(",").Distinct().ToImmutableList();

            var corsSettings = configuration.GetSetting<CorsSettings>();

            return corsSettings with { Origins = originsAsArray, Headers = headersAsArray };

        }

        public static void UseCorsConfiguration(this IApplicationBuilder app)
        {
            app.UseCors(CorsSettingsName);
        }
    }
}
