using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    /// <summary>
    /// Provides the extension to register the JSON serializer which we want to use
    /// in our API`s.
    /// </summary>
    public static class JsonSerializerExtension
    {
        /// <summary>
        /// Adds the <see cref="IJsonSerializer"/> to the service collection.
        /// </summary>
        public static void AddJsonSerializer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
        }
    }
}