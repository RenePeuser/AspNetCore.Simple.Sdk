using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Mediator;
using AspNetCore.Simple.Sdk.Serializer.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Caching
{
    public static class AddMediatRCachingExtension
    {
        public static void AddMediatRCaching(this IServiceCollection services, Assembly assembly)
        {
            services.AddMediator(assembly);
            services.AddKeyBuilder();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        }
    }

    public class CachingBehavior<TRequest, TResponse>(IKeyBuilder keyBuilder,
                                                      IJsonSerializer jsonSerializer,
                                                      ICachingService cachingService) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICachableQuery<TResponse>
        where TResponse : CachableObject
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestAsJson = jsonSerializer.Serialize(request);
            var keyInfo = new KeyInfo(typeof(TRequest).Name.ToLower(CultureInfo.InvariantCulture), requestAsJson);
            var key = keyBuilder.BuildKey(keyInfo);

            var result = await cachingService.GetOrAddAsync(key,
                async () => await next().ConfigureAwait(false),
                request.UseCache,
                request.CacheTime).ConfigureAwait(false);
            return result;
        }
    }
}
