using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Polly;
using AspNetCore.Simple.Sdk.Utils;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace AspNetCore.Simple.Sdk.Caching
{
    internal static class AddRedisConnectionExtension
    {
        public static void AddRedisConnection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingletonOption<RedisSettings>(configuration);

            services.AddSingletonIfNotExists<IRedisConnection, RedisConnection>();
        }
    }

    public interface IRedisConnection : IDisposable
    {
        Task StartupAsync();
        Task ReconnectAsync();
        Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> asyncFunc);
    }

    public static class AddRedisConnectionFactoryExtension
    {
        public static void AddRedisConnectionFactory(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBackOff();
            services.AddRedisSettings(configuration);

            services.AddSingletonIfNotExists<RedisConnectionFactory>();
        }
    }

    public sealed class RedisConnectionFactory
    {
        private readonly RedisSettings _redisSettings;
        private readonly IBackoff _backoff;
        private readonly ILogger<RedisConnectionFactory> _logger;

        public RedisConnectionFactory(RedisSettings redisSettings,
                                      IBackoff backoff,
                                      ILogger<RedisConnectionFactory> logger)
        {
            _redisSettings = redisSettings;
            _backoff = backoff;
            _logger = logger;
        }

        public IRedisConnection BuildWith()
        {
            return BuildWith(_redisSettings);
        }

        public IRedisConnection BuildWith(RedisSettings redisSettings)
        {
            return new RedisConnection(redisSettings, _logger, _backoff);
        }
    }

    /// <summary>
    /// We use recommended connection from Microsoft to connect to redis only modernized :)
    /// https://github.com/Azure-Samples/azure-cache-redis-samples/blob/main/quickstart/aspnet-core/ContosoTeamStats/RedisConnection.cs
    /// </summary>
    internal sealed class RedisConnection : DisposableBase, IRedisConnection
    {
        private readonly RedisSettings _redisSettings;
        private readonly ILogger _logger;
        private readonly IBackoff _backoff;
        private long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private DateTimeOffset _firstErrorTime = DateTimeOffset.MinValue;
        private DateTimeOffset _previousErrorTime = DateTimeOffset.MinValue;

        private readonly SemaphoreSlim _reconnectSemaphore = new(initialCount: 1, maxCount: 1);
        private ConnectionMultiplexer? _connection;
        private IDatabase? _database;
        private readonly AsyncRetryPolicy _redisRetryPolicy;

        // StackExchange.Redis will also be trying to reconnect internally,
        // so limit how often we recreate the ConnectionMultiplexer instance
        // in an attempt to reconnect
        private readonly TimeSpan _reconnectMinInterval = TimeSpan.FromSeconds(60);

        // If errors occur for longer than this threshold, StackExchange.Redis
        // may be failing to reconnect internally, so we'll recreate the
        // ConnectionMultiplexer instance
        private readonly TimeSpan _reconnectErrorThreshold = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _restartConnectionTimeout = TimeSpan.FromSeconds(15);

        public RedisConnection(RedisSettings redisSettings, ILogger logger, IBackoff backoff)
        {
            _redisSettings = redisSettings;
            _logger = logger;
            _backoff = backoff;

            _redisRetryPolicy = Policy.Handle<RedisConnectionException>()
                                      .Or<SocketException>()
                                      .Or<ObjectDisposedException>()
                                      .Or<RedisException>()
                                      .WaitAndRetryAsync(redisSettings.ConnectRetry, GetSleepDuration, RetryOnError);
        }

        private TimeSpan GetSleepDuration(int retry)
        {
            var minTimeout = _redisSettings.ConnectTimeout * retry;
            var maxTimeout = minTimeout * 2;

            var waitTimes = _backoff.DecorrelatedJitterBackoff(minTimeout, maxTimeout, _redisSettings.ConnectRetry);
            return waitTimes.ElementAt(retry - 1);
        }

        private Task RetryOnError(Exception exception, TimeSpan waitTime, int retry, Context context)
        {
            _logger.LogInformation($"Redis retry policy executed. {exception.Message}");
            return ReconnectInternalAsync(false, waitTime);
        }

        public Task StartupAsync()
        {
            return _redisRetryPolicy.ExecuteAsync(() => ReconnectInternalAsync(true, _redisSettings.ConnectTimeout));
        }

        public Task ReconnectAsync()
        {
            return _redisRetryPolicy.ExecuteAsync(() => ReconnectInternalAsync(false, _redisSettings.ConnectTimeout));
        }

        public Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> asyncFunc)
        {
            return _redisRetryPolicy.ExecuteAsync(() => asyncFunc(_database!)); // hint in case database is null it will be recreated
        }

        // Hint: Looks crazy this method, but imagine multiple request enter your API = multiple threads and if a connection or
        //       a redis action like setting a value or fetching went wrong this can be helpful
        /// https://github.com/Azure-Samples/azure-cache-redis-samples/blob/main/quickstart/aspnet-core/ContosoTeamStats/RedisConnection.cs
        /// <summary>
        /// Force a new ConnectionMultiplexer to be created.
        /// NOTES:
        ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen as a result of calling ForceReconnectAsync().
        ///     2. Call ForceReconnectAsync() for RedisConnectionExceptions and RedisSocketExceptions. You can also call it for RedisTimeoutExceptions,
        ///         but only if you're using generous ReconnectMinInterval and ReconnectErrorThreshold. Otherwise, establishing new connections can cause
        ///         a cascade failure on a server that's timing out because it's already overloaded.
        ///     3. The code will:
        ///         a. wait to reconnect for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
        ///         b. not reconnect more frequently than configured in "ReconnectMinInterval"
        /// </summary>
        /// <param name="initializing">Should only be true when ForceReconnect is running at startup.</param>
        /// <param name="connectTimeout">The timeout for redis for Polly retry to support best possible reconnect cases</param>
        private async Task ReconnectInternalAsync(bool initializing, TimeSpan connectTimeout)
        {
            var previousTicks = Interlocked.Read(ref _lastReconnectTicks);
            var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            var elapsedSinceLastReconnect = DateTimeOffset.UtcNow - previousReconnectTime;

            // We want to limit how often we perform this top-level reconnect, so we check how long it's been since our last attempt.
            if (elapsedSinceLastReconnect < _reconnectMinInterval)
            {
                return;
            }

            try
            {
                await _reconnectSemaphore.WaitAsync(_restartConnectionTimeout).ConfigureAwait(false);
            }
            catch
            {
                // If we fail to enter the semaphore, then it is possible that another thread has already done so.
                // ForceReconnectAsync() can be retried while connectivity problems persist.
                return;
            }

            try
            {
                var utcNow = DateTimeOffset.UtcNow;
                elapsedSinceLastReconnect = utcNow - previousReconnectTime;

                if (_firstErrorTime == DateTimeOffset.MinValue && !initializing)
                {
                    // We haven't seen an error since last reconnect, so set initial values.
                    _firstErrorTime = utcNow;
                    _previousErrorTime = utcNow;
                    return;
                }

                if (elapsedSinceLastReconnect < _reconnectMinInterval)
                {
                    return; // Some other thread made it through the check and the lock, so nothing to do.
                }

                var elapsedSinceFirstError = utcNow - _firstErrorTime;
                var elapsedSinceMostRecentError = utcNow - _previousErrorTime;

                var shouldReconnect =
                    elapsedSinceFirstError >= _reconnectErrorThreshold // Make sure we gave the multiplexer enough time to reconnect on its own if it could.
                    && elapsedSinceMostRecentError <= _reconnectErrorThreshold; // Make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).

                // Update the previousErrorTime timestamp to be now (e.g. this reconnect request).
                _previousErrorTime = utcNow;

                if (!shouldReconnect && !initializing)
                {
                    return;
                }

                _firstErrorTime = DateTimeOffset.MinValue;
                _previousErrorTime = DateTimeOffset.MinValue;

                if (_connection.IsNotNull())
                {
                    try
                    {
                        await _connection.CloseAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        // Ignore any errors from the old connection
                    }
                }

                Interlocked.Exchange(ref _connection, null);
                var newConnection = await ConnectionMultiplexer.ConnectAsync(_redisSettings.ConnectionString, config =>
                {
                    config.AsyncTimeout = _redisSettings.AsyncTimeout.TotalMilliseconds.ToInt();
                    config.SyncTimeout = _redisSettings.SyncTimeout.TotalMilliseconds.ToInt();
                    config.ConnectRetry = 1;
                    config.ConnectTimeout = connectTimeout.TotalMilliseconds.ToInt();

                }).ConfigureAwait(false);

                Interlocked.Exchange(ref _connection, newConnection);
                Interlocked.Exchange(ref _lastReconnectTicks, utcNow.UtcTicks);
                var newDatabase = _connection.GetDatabase();
                Interlocked.Exchange(ref _database, newDatabase);
            }
            finally
            {
                _reconnectSemaphore.Release();
            }
        }

        protected override void DisposeManagedResources()
        {
            try
            {
                _connection?.Dispose();
            }
            catch (Exception)
            {
                // We have to go sure that no exception occurred during dispose on any reason.
            }
        }
    }
}
