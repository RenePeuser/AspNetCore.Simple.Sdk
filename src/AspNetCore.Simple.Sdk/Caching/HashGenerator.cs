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
            using var hashAlgorithm = SHA512.Create();
            var byteValue = Encoding.UTF8.GetBytes(json);
            var byteHash = hashAlgorithm.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }
    }
}
