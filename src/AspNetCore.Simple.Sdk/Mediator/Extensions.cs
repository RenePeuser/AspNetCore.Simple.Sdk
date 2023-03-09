using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Mediator
{
    public static class AddMediatorExtension
    {
        internal static readonly ConcurrentDictionary<string, Assembly> RegisteredMediators = new();

        public static void AddMediator<TImplementation>(this IServiceCollection services, Type type) where TImplementation : class
        {
            services.AddMediator(typeof(TImplementation).Assembly);
        }

        public static void AddMediatorPipelineBehavior(this IServiceCollection services, Type serviceType, Type implementation)
        {
            services.AddTransient(serviceType, implementation);
        }

        public static void AddMediator(this IServiceCollection services, Type type)
        {
            services.AddMediator(type.Assembly);
        }

        public static void AddMediator(this IServiceCollection services)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            services.AddMediator(callingAssembly);
        }

        public static void AddMediator(this IServiceCollection services, Assembly assembly)
        {
            if (RegisteredMediators.ContainsKey(assembly.FullName!))
            {
                return;
            }

            RegisteredMediators.AddOrUpdate(assembly.FullName!, assembly, (_, __) => assembly);

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });
        }
    }

    /// <summary>
    /// Provides a set of extensions for <see cref="IMediator"/>
    /// </summary>
    public static class MediatorExtensions
    {

        /// <summary>
        /// Asynchronously send a request to a single handler
        /// </summary>
        /// <param name="mediator">The <see cref="IMediator"/></param>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        public static Task SendAsync(this IMediator mediator,
                                     IRequest request,
                                     CancellationToken cancellationToken = default)
        {
            return mediator.Send(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously send a request to a single handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="mediator">The <see cref="IMediator"/></param>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        public static Task<TResponse> SendAsync<TResponse>(this IMediator mediator,
                                                           IRequest<TResponse> request,
                                                           CancellationToken cancellationToken = default)
        {
            return mediator.Send(request, cancellationToken);

        }

        public static IAsyncEnumerable<TResponse> SendAsync<TResponse>(this IMediator mediator,
                                                           IStreamRequest<TResponse> request,
                                                           CancellationToken cancellationToken = default)
        {
            return mediator.CreateStream(request, cancellationToken);

        }

        /// <summary>
        /// Asynchronously send an object request to a single handler via dynamic dispatch
        /// </summary>
        /// <param name="mediator">The <see cref="IMediator"/> instance to extend.</param>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the send operation. The task result contains the type erased handler response</returns>
        public static Task<object?> SendAsync(this IMediator mediator,
                                              object request,
                                              CancellationToken cancellationToken = default)
        {
            return mediator.Send(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously send a notification to multiple handlers
        /// </summary>
        /// <param name="mediator">The <see cref="IMediator"/> instance to extend.</param>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the publish operation.</returns>
        public static Task PublishAsync(this IMediator mediator,
                                        object notification,
                                        CancellationToken cancellationToken = default)
        {
            return mediator.Publish(notification, cancellationToken);
        }

        /// <summary>
        /// Asynchronously send a notification to multiple handlers
        /// </summary>
        /// <param name="mediator">The <see cref="IMediator"/> instance to extend.</param>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the publish operation.</returns>
        public static Task PublishAsync<TNotification>(this IMediator mediator,
                                                       TNotification notification,
                                                       CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return mediator.Publish(notification, cancellationToken);
        }
    }
}
