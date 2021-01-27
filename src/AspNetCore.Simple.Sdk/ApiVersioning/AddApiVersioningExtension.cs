using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ApiVersioning
{
    public static class AddApiVersioningExtension
    {
        public static void AddApiVersioningSimplified(this IServiceCollection services)
        {
            services.AddApiVersioning(apiVersionOptions =>
            {
                apiVersionOptions.DefaultApiVersion = new ApiVersion(0, 1);
                apiVersionOptions.AssumeDefaultVersionWhenUnspecified = true;
            });
        }
    }
}
