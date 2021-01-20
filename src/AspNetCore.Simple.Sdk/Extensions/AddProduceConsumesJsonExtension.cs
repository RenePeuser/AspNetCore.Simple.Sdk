using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class AddProduceConsumesJsonExtension
    {
        public static void AddJsonContentNegotiation(this IServiceCollection services)
        {
            // Temp ignore till it works with multiple root attributes
            services.AddMvc(config =>
            {
                // this avoids to decorate all our controllers and http action methods with this attribute
                config.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
                config.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
            });
        }
    }
}
