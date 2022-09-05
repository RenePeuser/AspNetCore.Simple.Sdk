using AspNetCore.Simple.Sdk.Automapper;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Authentication.Auth0
{
    public static class AddAuth0SettingsExtensions
    {
        public static void AddAuth0Settings(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingletonOption<Auth0>(configuration);
        }
    }

    public record Auth0
    {
        public string Authority { get; init; } = "https://bit-ba-dev.eu.auth0.com/";
        public string TokenEndpoint { get; init; } = "https://bit-ba-dev.eu.auth0.com/oauth/token";
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
        public string GrantType { get; init; } = "client_credentials";
        public string Audience { get; init; } = string.Empty;
        public int TokenCacheTimeInHours { get; init; } = 8;
    }

    public static class AddOAuthAuthenticationExtension
    {
        public static void AddOAuthAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // If Auth0 settings does not exists we do not activate it
            if (configuration.TryGetSettings<Auth0>(out var auth0Settings).IsFalse())
            {
                return;
            }

            if (services.IsAlreadyRegistered<Auth0>())
            {
                return;
            }

            services.AddAutoMapper();

            services.AddSingletonIfNotExists(auth0Settings);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = auth0Settings.Authority;
                options.Audience = auth0Settings.Audience;
            });
        }
    }
}
