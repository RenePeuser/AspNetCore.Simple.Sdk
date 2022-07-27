using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public static class Audiences
    {
        public static string ComponentInternal => "component-internal";
        public static string BusinessUnitInternal => "business-unit-internal";
        public static string CompanyInternal => "company-internal";
        public static string ExternalPartner => "external-partner";
        public static string ExternalPublic => "external-public";
    }

    public record SwaggerInfo
    {
        /// <summary>See Zalando open source <a href="https://opensource.zalando.com/restful-api-guidelines/#215"> guideline 215</a>.</summary>
        public Guid? Id { get; init; }

        /// <summary>See Zalando open source <a href="https://opensource.zalando.com/restful-api-guidelines/#219"> guideline 219</a>.</summary>
        public string Audience { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string ContactName { get; init; } = string.Empty;

        public string ContactEmail { get; init; } = string.Empty;

        public Uri? ContactUrl { get; init; }
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
            var infoExtension = GetExtensionInfo(swaggerInfo).ToDictionary(item => item.key, item => item.openApiExtension);
            var info = new OpenApiInfo
            {
                Title = swaggerInfo.Title,
                Version = $"{apiVersion.MajorVersion}.{apiVersion.MinorVersion}",
                Description = swaggerInfo.Description,
                Contact = new OpenApiContact()
                {
                    Email = swaggerInfo.ContactEmail,
                    Name = swaggerInfo.ContactName,
                    Url = swaggerInfo.ContactUrl
                },

                Extensions = infoExtension
            };

            return info;
        }

        private static IEnumerable<(string key, IOpenApiExtension openApiExtension)> GetExtensionInfo(SwaggerInfo swaggerInfo)
        {
            if (swaggerInfo.Id is not null)
            {
                yield return ("x-api-id", new OpenApiString(swaggerInfo.Id.ToString()));
            }

            if (swaggerInfo.Audience is not null)
            {
                yield return ("x-audience", new OpenApiString(swaggerInfo.Audience));
            }
        }
    }
}
