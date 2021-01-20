using System.Text.Json;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public class JsonSerializer : IJsonSerializer
    {
        private static readonly JsonSerializerOptions _serializeOptions;

        static JsonSerializer()
        {
            // hint here we can setup default behavior for serializing. For performance issue DO NOT use WriteIndented = true !
            // For RessourceFull it cost 6-10ms just the formatting if you want a nice formatting for your json use:
            // https://jsonformatter.curiousconcept.com/ or https://jsonformatter.org/
            _serializeOptions = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
        }

        public string Serialize<T>(T source)
        {
            return System.Text.Json.JsonSerializer.Serialize(source, _serializeOptions);
        }

        public T Deserialize<T>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, _serializeOptions);
        }
    }
}