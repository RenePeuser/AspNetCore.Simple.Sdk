using System;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T source);

        T Deserialize<T>(string json);

        T? DeserializeOrDefault<T>(string json, T? defaultValue = default);

        object Deserialize<T>(string json, Type responseType);

        object? DeserializeOrDefault(string json, Type responseType, object? defaultValue = default);
    }
}
