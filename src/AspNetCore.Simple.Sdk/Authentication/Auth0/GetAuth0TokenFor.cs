using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Caching;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;

namespace AspNetCore.Simple.Sdk.Authentication.Auth0
{
    public record GetAuth0TokenFor(Auth0 Auth0Settings, bool UseCache = true) : CachableQuery<Auth0Token>(Auth0Settings.TokenCacheTime, UseCache);

    internal sealed class GetAuth0TokenForHandler : IQueryHandler<GetAuth0TokenFor, Auth0Token>
    {
        private readonly IHttpClientFactory _htpHttpClientFactory;

        public GetAuth0TokenForHandler(IHttpClientFactory htpHttpClientFactory)
        {
            _htpHttpClientFactory = htpHttpClientFactory;
        }

        public async Task<Auth0Token> Handle(GetAuth0TokenFor request, CancellationToken cancellationToken)
        {
            var auth0Request = new Auth0Request()
            {
                Audience = request.Auth0Settings.Audience,
                ClientId = request.Auth0Settings.ClientId,
                ClientSecret = request.Auth0Settings.ClientSecret,
                GrantType = request.Auth0Settings.GrantType
            };

            var client = _htpHttpClientFactory.CreateClient();
            var response = await client.PostAsJsonStringAsync(request.Auth0Settings.TokenEndpoint, auth0Request.ToJson()).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var auth0TokenReponse = await response.Content.ReadFromJsonAsync<Auth0TokenReponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
                if (auth0TokenReponse.IsNull())
                {
                    // No more details possible to print out in exception message, cause can contains secret infos !!
                    throw new ProblemDetailsException("Invalid Auth0TokenReponse was returned");
                }

                // var authToken = _mapper.Map<Auth0Token>(auth0TokenReponse);
                var authToken = new Auth0Token()
                {
                    Scope = auth0TokenReponse.Scope,
                    Token = auth0TokenReponse.Token,
                    TokenType = auth0TokenReponse.TokenType,
                    ExpiresOnUtc = DateTime.UtcNow.Add(TimeSpan.FromSeconds(auth0TokenReponse.ExpiresInSeconds)),
                    TokenReadyToUse = $"{auth0TokenReponse.TokenType} {auth0TokenReponse.Token}"
                };


                return authToken;
            }

            throw new ProblemDetailsException("Unable to fetch token from Auth0", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
