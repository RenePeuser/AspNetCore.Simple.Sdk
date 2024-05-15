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
                PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, NumberHandling = JsonNumberHandling.AllowReadingFromString, Converters = { new JsonStringEnumConverter() }
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
            T? deserializeResult = default;
            var errorMessage = string.Empty;
            try
            {
                deserializeResult = System.Text.Json.JsonSerializer.Deserialize<T>(json, _serializeOptions);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }

            if (deserializeResult.IsNull())
            {
                var securityCritical = typeof(T).GetCustomAttribute<ShowJsonOnErrorAttribute>();
                var jsonString = securityCritical.IsNotNull() ? json : "Hidden cause of security critical infos";

                throw new ProblemDetailsException("Could not deserialize your json string into expected type",
                    $"Could not deserialize your json string into expected type: {typeof(T).Name}",
                    ("Exception", errorMessage),
                    ("JsonString", jsonString),
                    ("Type", typeof(T).Name),
                    ("TypeFullName", typeof(T).FullName ?? string.Empty),
                    ("Info", $"Add [{nameof(ShowJsonOnErrorAttribute)}] to your type to see json. But be careful of security critical infos"));
            }

            return deserializeResult;
        }

        public T? DeserializeOrDefault<T>(string json, T? defaultValue = default)
        {
            try
            {
                var deserializeResult = System.Text.Json.JsonSerializer.Deserialize<T>(json, _serializeOptions);
                return deserializeResult;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public object Deserialize<T>(string json, Type returnType)
        {
            object? deserializeResult = default(T);
            var errorMessage = string.Empty;
            try
            {
                deserializeResult = System.Text.Json.JsonSerializer.Deserialize(json, returnType, _serializeOptions);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }


            if (deserializeResult.IsNull())
            {
                var securityCritical = typeof(T).GetCustomAttribute<ShowJsonOnErrorAttribute>();
                var jsonString = securityCritical.IsNotNull() ? json : "Hidden cause of security critical infos";

                throw new ProblemDetailsException("Could not deserialize your json string into expected type",
                    $"Could not deserialize your json string into expected type: {typeof(T).Name}",
                    ("Exception", errorMessage),
                    ("JsonString", jsonString),
                    ("Type", typeof(T).Name),
                    ("TypeFullName", typeof(T).FullName ?? string.Empty),
                    ("Info", $"Add [{nameof(ShowJsonOnErrorAttribute)}] to your type to see json. But be careful of security critical infos"));
            }

            return deserializeResult;
        }

        public object? DeserializeOrDefault(string json, Type returnType, object? defaultValue = default)
        {
            try
            {
                var deserializeResult = System.Text.Json.JsonSerializer.Deserialize(json, returnType, _serializeOptions);
                return deserializeResult;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
