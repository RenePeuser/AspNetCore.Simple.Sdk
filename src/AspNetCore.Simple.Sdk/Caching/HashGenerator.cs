using System;
using System.Security.Cryptography;
using System.Text;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Caching
{
    internal interface IHashGenerator
    {
        string ComputeHash<T>(T source) where T : class;
    }

    internal static class AddHashGeneratorExtension
    {
        public static void AddHashGenerator(this IServiceCollection services)
        {
            services.AddJsonSerializer();

            services.AddSingletonIfNotExists<IHashGenerator, HashGenerator>();
        }
    }

    internal class HashGenerator : IHashGenerator
    {
        private readonly IJsonSerializer _jsonSerializer;

        public HashGenerator(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public string ComputeHash<T>(T source) where T : class
        {
            var json = _jsonSerializer.Serialize(source);
            using var md5CryptoProvider = new SHA512CryptoServiceProvider();
            var data = md5CryptoProvider.ComputeHash(Encoding.ASCII.GetBytes(json));
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }
    }
}
