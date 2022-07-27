using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public static class JsonSerializerExtension
    {
        public static void AddJsonSerializer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();

            serviceCollection.AddMvc()
                             .AddJsonOptions(opts =>
                             {
                                 var enumConverter = new JsonStringEnumConverter();
                                 opts.JsonSerializerOptions.Converters.Add(enumConverter);
                             });
        }
    }
}
