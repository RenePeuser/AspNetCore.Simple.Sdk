using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class AddProduceConsumesJsonExtension
    {
        public static void AddJsonContentNegotiation(this IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                config.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
                config.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
            });
        }
    }
}
