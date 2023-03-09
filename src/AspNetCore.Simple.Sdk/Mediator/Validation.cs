using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Mediator
{
    public static class AddValidationBehaviorExtension
    {
        public static void AddValidationBehavior(this IServiceCollection services)
        {
            services.AddMediatorPipelineBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }
    }

    public interface IRequestValidator<in TRequest> where TRequest : IBaseRequest
    {
        Task ValidateAsync(TRequest request);
    }


    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationBehavior(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // 1. Check if a validator is available
            var validatorForRequest = _serviceProvider.GetService<IRequestValidator<TRequest>>();
            if (validatorForRequest is not null)
            {
                await validatorForRequest.ValidateAsync(request).ConfigureAwait(false);
            }

            return await next().ConfigureAwait(false);
        }
    }
}
