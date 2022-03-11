using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.ApiVersioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public static class AddSwaggerGenExtensions
    {
        public static void AddSwaggerGenSimplified(this IServiceCollection services, Assembly assembly, string swaggerUiTitle)
        {
            var apiVersionProvider = new ApiVersionProvider();
            var allApiVersions = apiVersionProvider.GetAllApiVersions(assembly);

            services.AddSwaggerGen(options =>
            {
                options.DocInclusionPredicate((_, _) => true);
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                options.AddSwaggerGrouping();
                options.OperationFilter<RemoveVersionParameterFilter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
                options.DocumentFilter<AdditionalPropertiesFilter>();
                options.AddBearerSecurityDefinition();
                options.AddBearerSecurityRequirement();
                options.AddXmlComments(assembly);

                foreach (var apiVersion in allApiVersions)
                {
                    options.SwaggerDoc($"v{apiVersion.MajorVersion}.{apiVersion.MinorVersion}", new OpenApiInfo { Title = swaggerUiTitle, Version = $"v{apiVersion.MajorVersion}" });
                }
            });
        }
    }
}
