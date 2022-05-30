using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Serializer.Json;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Caching
{
    public static class AddMediatRCachingExtension
    {
        public static void AddMediatRCaching(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
        {
            services.AddMediatR(assembly);
            services.AddKeyBuilder();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        }
    }

    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICachableQuery<TResponse>
                                                                                               where TResponse : CachableObject
    {
        private readonly IKeyBuilder _keyBuilder;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ICachingService _cachingService;

        public CachingBehavior(IKeyBuilder keyBuilder,
                               IJsonSerializer jsonSerializer,
                               ICachingService cachingService)
        {
            _keyBuilder = keyBuilder;
            _jsonSerializer = jsonSerializer;
            _cachingService = cachingService;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var requestAsJson = _jsonSerializer.Serialize(request);
            var keyInfo = new KeyInfo(typeof(TRequest).Name.ToLower(CultureInfo.InvariantCulture), requestAsJson);
            var key = _keyBuilder.BuildKey(keyInfo);

            var result = await _cachingService.GetOrAddAsync(key,
                                                             async () => await next().ConfigureAwait(false),
                                                             request.UseCache,
                                                             request.CacheTime).ConfigureAwait(false);
            return result;
        }
    }
}
