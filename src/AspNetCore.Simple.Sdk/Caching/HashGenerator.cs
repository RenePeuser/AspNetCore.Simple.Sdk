using System;
using System.Security.Cryptography;
using System.Text;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
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

    internal sealed class HashGenerator : IHashGenerator
    {
        private readonly IJsonSerializer _jsonSerializer;

        public HashGenerator(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public string ComputeHash<T>(T source) where T : class
        {
            var json = _jsonSerializer.Serialize(source);
            var byteValue = Encoding.UTF8.GetBytes(json);
            var byteHash = SHA512.HashData(byteValue);
            return Convert.ToBase64String(byteHash);
        }
    }
}
