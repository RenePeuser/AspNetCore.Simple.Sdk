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
                // apiVersionOptions.ApiVersionReader = new UrlSegmentApiVersionReader(); -> Has not effect for attribute [ApiVersion("1.0")]
                apiVersionOptions.DefaultApiVersion = new ApiVersion(1, 0);
                apiVersionOptions.AssumeDefaultVersionWhenUnspecified = true;
                apiVersionOptions.ReportApiVersions = true;
                apiVersionOptions.UseApiBehavior = true;
            });
        }
    }
}
