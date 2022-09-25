using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Caching;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Mediator;
using AutoMapper;

namespace AspNetCore.Simple.Sdk.Authentication.Auth0
{
    public record GetAuth0TokenFor(Auth0 Auth0Settings, bool UseCache = true) : CachableQuery<Auth0Token>(Auth0Settings.TokenCacheTime, UseCache);

    internal class GetAuth0TokenForHandler : IQueryHandler<GetAuth0TokenFor, Auth0Token>
    {
        private readonly IHttpClientFactory _htpHttpClientFactory;
        private readonly IMapper _mapper;

        public GetAuth0TokenForHandler(IHttpClientFactory htpHttpClientFactory,
                                       IMapper mapper)
        {
            _htpHttpClientFactory = htpHttpClientFactory;
            _mapper = mapper;
        }

        public async Task<Auth0Token> Handle(GetAuth0TokenFor request, CancellationToken cancellationToken)
        {
            var auth0Request = _mapper.Map<Auth0Request>(request.Auth0Settings);

            var client = _htpHttpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(request.Auth0Settings.TokenEndpoint, auth0Request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var auth0TokenReponse = await response.Content.ReadFromJsonAsync<Auth0TokenReponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
                var authToken = _mapper.Map<Auth0Token>(auth0TokenReponse);
                return authToken;
            }

            throw new ProblemDetailsException("Unable to fetch token from Auth0", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
