using System;
using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public record SwaggerInfo
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string ContactName { get; init; } = string.Empty;
        public string ContactEmail { get; init; } = string.Empty;
        public string ContactUrl { get; init; } = string.Empty;
    }

    public static class AddSwaggerGenExtensions
    {
        public static void AddSwaggerGenSimplified(this IServiceCollection services, Assembly assembly, IConfiguration configuration)
        {
            var swaggerInfo = configuration.GetSetting<SwaggerInfo>();
            swaggerInfo = swaggerInfo is null ? new SwaggerInfo() : swaggerInfo;

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
                options.DocumentFilter<RootLevelTagsFilter>();
                options.AddBearerSecurityDefinition();
                options.AddBearerSecurityRequirement();
                options.AddXmlComments(assembly);
                options.CustomSchemaIds(type => type.ToString());
                // Conflicts with ExtensibleEnumFilter. Use one or the other.
                // options.SchemaFilter<EnumSchemaFilter>();
                options.SchemaFilter<ExtensibleEnumFilter>();
                options.ParameterFilter<ExtensibleEnumFilter>();
                options.EnableAnnotations();  // necessary to include the SwaggerOperationAttribute.OperationIds in the Swagger Json

                foreach (var apiVersion in allApiVersions)
                {
                    var openApiInfo = GetVersionSpecificApiInfo(swaggerInfo, apiVersion);

                    options.SwaggerDoc($"v{apiVersion.MajorVersion}.{apiVersion.MinorVersion}", openApiInfo);
                }
            });
        }

        private static OpenApiInfo GetVersionSpecificApiInfo(SwaggerInfo swaggerInfo, ApiVersion apiVersion)
        {
            var info = new OpenApiInfo
            {
                Title = swaggerInfo.Title,
                Version = $"v{apiVersion.MajorVersion}",
                Description = swaggerInfo.Description,
                Contact = new OpenApiContact()
                {
                    Email = swaggerInfo.ContactEmail,
                    Name = swaggerInfo.ContactName,
                    Url = swaggerInfo.ContactUrl.IsNullOrWhiteSpace() ? null : new Uri(swaggerInfo.ContactUrl)
                }
            };

            return info;
        }
    }
}
