using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public static class JsonSerializerExtension
    {
        public static void AddJsonSerializer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
        }
    }
}
