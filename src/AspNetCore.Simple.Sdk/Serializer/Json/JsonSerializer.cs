using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _serializeOptions;

        public JsonSerializer()
        {
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

        public T? Deserialize<T>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, _serializeOptions);
        }

        public object? Deserialize(string json, Type responseType)
        {
            return System.Text.Json.JsonSerializer.Deserialize(json, responseType, _serializeOptions);
        }
    }
}
