namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T source);

        T Deserialize<T>(string json);
    }
}