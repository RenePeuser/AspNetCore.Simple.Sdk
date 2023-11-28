using System;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.CorrelationId
{
    public static class AddCorrelationIdMiddlewareExtension
    {
        public static void AddCorrelationIdMiddleware(this IServiceCollection services)
        {
            services.AddCorrelationIdService();

            services.AddSingletonIfNotExists<CorrelationIdMiddleware>();
        }

        public static void UseCorrelationIdMiddleware(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }

    internal sealed class CorrelationIdMiddleware : IMiddleware
    {
        private readonly ICorrelationIdService _correlationIdService;
        private const string CorrelationIdHeader = "x-correlation-id";

        public CorrelationIdMiddleware(ICorrelationIdService correlationIdService)
        {
            _correlationIdService = correlationIdService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Headers.ContainsKey(CorrelationIdHeader).IsFalse())
            {
                context.Request.Headers.Append(CorrelationIdHeader, _correlationIdService.CreateId());
            }

            await next(context).ConfigureAwait(false);
        }
    }

    public static class AddCorrelationIdServiceExtension
    {
        public static void AddCorrelationIdService(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ICorrelationIdService, CorrelationIdService>();
        }
    }

    public interface ICorrelationIdService
    {
        string CreateId();
    }

    internal sealed class CorrelationIdService : ICorrelationIdService
    {
        public string CreateId()
        {
            return Guid.NewGuid().ToString();
        }
    }

}
