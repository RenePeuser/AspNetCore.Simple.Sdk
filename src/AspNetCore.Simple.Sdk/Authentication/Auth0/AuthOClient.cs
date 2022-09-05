using System;
using System.Text.Json.Serialization;
using AspNetCore.Simple.Sdk.Caching;
using AutoMapper;

namespace AspNetCore.Simple.Sdk.Authentication.Auth0
{
    public static class AddAuth0ResponseMappingExtension
    {
        public static void AddAuth0ResponseMapping(this IMapperConfigurationExpression mapperConfigurationExpression)
        {
            mapperConfigurationExpression.CreateMap<Auth0TokenReponse, Auth0Token>()
                                         .ForMember(authTokenResponse => authTokenResponse.ExpiresOnUtc, token => token.MapFrom(t => DateTime.UtcNow.Add(TimeSpan.FromSeconds(t.ExpiresInSeconds))))
                                         .ForMember(authTokenResponse => authTokenResponse.TokenReadyToUse, token => token.MapFrom(t => $"{t.TokenType} {t.Token}"));
        }

        public static void AddAuth0RequestMapping(this IMapperConfigurationExpression mapperConfigurationExpression)
        {
            mapperConfigurationExpression.CreateMap<Auth0, Auth0Request>();
        }
    }

    public record Auth0Token : CachableObject
    {
        public string Token { get; init; } = string.Empty;

        public string TokenReadyToUse { get; init; } = string.Empty;

        public string Scope { get; init; } = string.Empty;

        public DateTimeOffset ExpiresOnUtc { get; init; }

        public string TokenType { get; init; } = string.Empty;
    }

    public record Auth0TokenReponse
    {
        [JsonPropertyName("access_token")]
        public string Token { get; init; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope { get; init; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresInSeconds { get; init; } = 86400;

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = string.Empty;
    }

    public record Auth0Request
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = string.Empty;

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; init; } = string.Empty;

        [JsonPropertyName("audience")]
        public string Audience { get; init; } = string.Empty;

        [JsonPropertyName("grant_type")]
        public string GrantType { get; init; } = string.Empty;
    }
}
