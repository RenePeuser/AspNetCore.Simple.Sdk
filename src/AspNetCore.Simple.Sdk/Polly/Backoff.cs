using System;
using System.Collections.Generic;
using System.Linq;
using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Polly
{
    public static class AddBackOffExtension
    {
        public static void AddBackOff(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IBackoff, Backoff>();
        }
    }

    public interface IBackoff
    {
        IEnumerable<TimeSpan> DecorrelatedJitterBackoff(TimeSpan minDelay, TimeSpan maxDelay, int retryCount, int? seed = null, bool fastFirst = false);
    }

    public class Backoff : IBackoff
    {
        public IEnumerable<TimeSpan> DecorrelatedJitterBackoff(TimeSpan minDelay, TimeSpan maxDelay, int retryCount, int? seed = null, bool fastFirst = false)
        {
            Throw.IfLessThan(minDelay, TimeSpan.Zero);
            Throw.IfLessThan(maxDelay, minDelay);
            Throw.IfLessThan(retryCount, 0);

            if (retryCount == 0)
            {
                return Enumerable.Empty<TimeSpan>();
            }

            return Enumerate(minDelay, maxDelay, retryCount, fastFirst, new ConcurrentRandom(seed));

            static IEnumerable<TimeSpan> Enumerate(TimeSpan min, TimeSpan max, int retry, bool fast, ConcurrentRandom random)
            {
                var i = 0;
                if (fast)
                {
                    i++;
                    yield return TimeSpan.Zero;
                }

                // https://github.com/aws-samples/aws-arch-backoff-simulator/blob/master/src/backoff_simulator.py#L45
                // self.sleep = min(self.cap, random.uniform(self.base, self.sleep * 3))

                // Formula avoids hard clamping (which empirically results in a bad distribution)
                var ms = min.TotalMilliseconds;
                for (; i < retry; i++)
                {
                    var ceiling = Math.Min(max.TotalMilliseconds, ms * 3);
                    ms = random.Uniform(min.TotalMilliseconds, ceiling);

                    yield return TimeSpan.FromMilliseconds(ms);
                }
            }
        }

        /// <summary>
        /// A random number generator with a Uniform distribution that is thread-safe (via locking).
        /// Can be instantiated with a custom <see cref="int"/> seed to make it emit deterministically.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private sealed class ConcurrentRandom
        {
            // Singleton approach is per MS best-practices.
            // https://docs.microsoft.com/en-us/dotnet/api/system.random?view=netframework-4.7.2#the-systemrandom-class-and-thread-safety
            // https://stackoverflow.com/a/25448166/
            // Also note that in concurrency testing, using a 'new Random()' for every thread ended up
            // being highly correlated. On NetFx this is maybe due to the same seed somehow being used
            // in each instance, but either way the singleton approach mitigated the problem.
            private static readonly Random Random = new();
            private readonly Random _randomLock;

            /// <summary>
            /// Creates an instance of the <see cref="ConcurrentRandom"/> class.
            /// </summary>
            /// <param name="seed">An optional <see cref="System.Random"/> seed to use.
            /// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
            public ConcurrentRandom(int? seed = null)
            {
                this._randomLock = seed == null
                    ? Random // Do not use 'new Random()' here; in concurrent scenarios they could have the same seed
                    : new Random(seed.Value);
            }

            /// <summary>
            /// Returns a random floating-point number that is greater than or equal to 0.0,
            /// and less than 1.0.
            /// This method uses locks in order to avoid issues with concurrent access.
            /// </summary>
            /// <returns>Nächste Zufallszahl.</returns>
            public double NextDouble()
            {
                // It is safe to lock on _random since it's not exposed
                // to outside use so it cannot be contended.
                lock (this._randomLock)
                {
                    return this._randomLock.NextDouble();
                }
            }

            /// <summary>
            /// Returns a random floating-point number that is greater than or equal to <paramref name="a"/>,
            /// and less than <paramref name="b"/>.
            /// </summary>
            /// <param name="a">The minimum value.</param>
            /// <param name="b">The maximum value.</param>
            /// <returns>Random value between <paramref name="a"/> and <paramref name="b"/></returns>
            public double Uniform(double a, double b)
            {
                if (a.EqualsTo(b))
                {
                    return a;
                }

                return a + ((b - a) * this.NextDouble());
            }
        }
    }
}
