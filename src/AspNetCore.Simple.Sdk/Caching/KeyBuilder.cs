using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Caching
{
    public static class AddKeyBuilderExtension
    {
        public static void AddKeyBuilder(this IServiceCollection services)
        {
            services.AddHashGenerator();

            services.AddSingletonIfNotExists<IKeyBuilder, HashKeyBuilder>();
        }
    }

    public record KeyInfo(string Name, params object[] Objects);

    public interface IKeyBuilder
    {
        string BuildKey(KeyInfo keyInfo);
    }

    internal sealed class HashKeyBuilder : IKeyBuilder
    {
        private readonly IHashGenerator _hashGenerator;

        public HashKeyBuilder(IHashGenerator hashGenerator)
        {
            _hashGenerator = hashGenerator;
        }

        public string BuildKey(KeyInfo keyInfo)
        {
            var hash = _hashGenerator.ComputeHash(keyInfo);
            return $"{keyInfo.Name}-{hash}";
        }
    }
}
