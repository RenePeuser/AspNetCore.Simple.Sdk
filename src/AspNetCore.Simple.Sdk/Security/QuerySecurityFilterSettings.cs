using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Security
{
    public static class AddQuerySecurityFilterSettingsExtension
    {
        public static void AddQuerySecurityFilterSettings(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.TryGetSettings<QuerySecurityFilterSettings>(out var querySecurityFilterSettings))
            {
                var queryParams = configuration.GetSection($"{nameof(QuerySecurityFilterSettings)}:{nameof(QuerySecurityFilterSettings.QueryParamsToIgnore)}").Value ?? string.Empty;
                var updateQuerySecurityFilterSettings = querySecurityFilterSettings with { QueryParamsToIgnore = queryParams.Split(";").ToImmutableList() };

                services.AddSingletonIfNotExists(updateQuerySecurityFilterSettings);
            }
            else
            {
                var defaultQuerySecurityFilterSettings = new QuerySecurityFilterSettings();
                services.AddSingletonIfNotExists(defaultQuerySecurityFilterSettings);
            }
        }
    }

    public record QuerySecurityFilterSettings
    {
        public IImmutableList<string> QueryParamsToIgnore { get; init; } = ImmutableList<string>.Empty;
    }
}
