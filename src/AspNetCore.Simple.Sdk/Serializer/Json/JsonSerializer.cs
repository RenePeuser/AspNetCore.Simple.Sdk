using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly ILogger<JsonSerializer> _logger;
        private readonly JsonSerializerOptions _serializeOptions;

        public JsonSerializer(ILogger<JsonSerializer> logger)
        {
            _logger = logger;
            _serializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public string Serialize<T>(T source)
        {
            return System.Text.Json.JsonSerializer.Serialize(source, _serializeOptions);
        }

        public string? SerializeOrDefault<T>(T source, string? defaultValue = default)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(source, _serializeOptions);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return defaultValue;
            }
        }

        public T Deserialize<T>(string json)
        {
            var deserializeResult = System.Text.Json.JsonSerializer.Deserialize<T>(json, _serializeOptions);
            if (deserializeResult.IsNull())
            {
                throw new ProblemDetailsException("Could not deserialize your json string into expected type",
                                                  $"Could not deserialize your json string into expected type: {typeof(T).Name}",
                                                  ("Json string", json),
                                                  ("Type", typeof(T).Name),
                                                  ("Type Fullname", typeof(T).FullName ?? string.Empty));
            }

            return deserializeResult;
        }

        public T? DeserializeOrDefault<T>(string json, T? defaultValue = default(T))
        {
            var deserializeResult = System.Text.Json.JsonSerializer.Deserialize<T>(json, _serializeOptions);
            return deserializeResult ?? defaultValue;
        }

        public object Deserialize<T>(string json, Type responseType)
        {
            var deserializeResult = System.Text.Json.JsonSerializer.Deserialize(json, responseType, _serializeOptions);
            if (deserializeResult.IsNull())
            {
                throw new ProblemDetailsException("Could not deserialize your json string into expected type",
                                                  $"Could not deserialize your json string into expected type: {typeof(T).Name}",
                                                  ("Json string", json),
                                                  ("Type", typeof(T).Name),
                                                  ("Type Fullname", typeof(T).FullName ?? string.Empty));
            }

            return deserializeResult;
        }

        public object? DeserializeOrDefault(string json, Type responseType, object? defaultValue = default)
        {
            var deserializeResult = System.Text.Json.JsonSerializer.Deserialize(json, responseType, _serializeOptions);
            return deserializeResult ?? defaultValue;
        }
    }
}
