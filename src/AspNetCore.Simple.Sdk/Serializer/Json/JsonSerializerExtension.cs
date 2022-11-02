using System.Text.Json.Serialization;
using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public static class JsonSerializerExtension
    {
        public static void AddJsonSerializer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingletonIfNotExists<IJsonSerializer, JsonSerializer>();

            serviceCollection.AddMvc()
                             .AddJsonOptions(opts =>
                             {
                                 var enumConverter = new JsonStringEnumConverter();
                                 opts.JsonSerializerOptions.Converters.Add(enumConverter);
                             });
        }
    }
}
