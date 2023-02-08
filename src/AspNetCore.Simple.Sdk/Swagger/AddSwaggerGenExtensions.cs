using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.Authentication.Auth0;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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


    public record SwaggerInfos
    {
        public SwaggerInfo[] SwaggerInfosByVersion { get; init; } = Array.Empty<SwaggerInfo>(); // only [] cause of configuration does not support IImmutableList
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

        public bool WithServerInfo { get; init; }

        public string? ContactUrl { get; init; }

        public string Version { get; init; } = "1.0";
    }

    public static class AddSwaggerGenExtensions
    {
        public static void AddSwaggerGenSimplified(this IServiceCollection services, Assembly assembly, IConfiguration configuration)
        {
            var swaggerInfo = configuration.GetSetting<SwaggerInfos>() ?? new SwaggerInfos();
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
                options.DocumentFilter<SchemaFilterForCurrentVersion>();
                options.SupportNonNullableReferenceTypes();

                // ToDo: think about next version strategy how to switch 
                if (configuration.TryGetSettings<Auth0>(out _))
                {
                    options.DocumentFilter<OAuth2Filter>();
                }
                else
                {
                    options.AddBearerSecurityDefinition();
                    options.AddBearerSecurityRequirement();
                }

                options.AddXmlComments(assembly);
                options.CustomSchemaIds(type => type.ToString());

                // EnumSchemaFilter conflicts with ExtensibleEnumFilter. Use one or the other.
                // options.SchemaFilter<EnumSchemaFilter>();
                options.SchemaFilter<ExtensibleEnumFilter>();

                options.ParameterFilter<ExtensibleEnumFilter>();
                options.EnableAnnotations();  // necessary to include the SwaggerOperationAttribute.OperationIds in the Swagger Json

                foreach (var apiVersion in allApiVersions)
                {
                    var openApiInfo = GetVersionSpecificApiInfo(swaggerInfo, apiVersion);

                    options.SwaggerDoc($"v{apiVersion.MajorVersion}.{apiVersion.MinorVersion}", openApiInfo);
                }

                options.MapType<DateOnly>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "date",
                    Example = new OpenApiString("2023-11-17")
                });
            });
        }

        private static OpenApiInfo GetVersionSpecificApiInfo(SwaggerInfos swaggerInfos, ApiVersion apiVersion)
        {
            var versionSpecificSwaggerInfo = swaggerInfos.SwaggerInfosByVersion.FirstOrDefault(swagger =>
            {
                var apiVersionSwaggerInfo = ToApiVersion(swagger.Version);
                return apiVersionSwaggerInfo == apiVersion;
            });

            if (versionSpecificSwaggerInfo.IsNull())
            {
                // Fallback no infos if nothing was found
                Debug.WriteLine($"No specific swagger info was found for api version: {apiVersion}. Please check your appsettings.json, environment variables for a correct declaration to get swagger infos per version");
                var sample = new SwaggerInfos() { SwaggerInfosByVersion = new SwaggerInfo[] { new SwaggerInfo() } }.ToJson();
                Debug.WriteLine(System.Text.Json.JsonSerializer.Serialize(JToken.Parse(sample).ToString(Formatting.Indented)));
                versionSpecificSwaggerInfo = new SwaggerInfo();
            }

            var infoExtension = GetExtensionInfo(versionSpecificSwaggerInfo).ToDictionary(item => item.key, item => item.openApiExtension);
            var info = new OpenApiInfo
            {
                Title = versionSpecificSwaggerInfo.Title,
                Version = $"{apiVersion.MajorVersion}.{apiVersion.MinorVersion}",
                Description = versionSpecificSwaggerInfo.Description,
                Contact = new OpenApiContact
                {
                    Email = versionSpecificSwaggerInfo.ContactEmail,
                    Name = versionSpecificSwaggerInfo.ContactName,
                    Url = versionSpecificSwaggerInfo.ContactUrl is null ? null : new Uri(versionSpecificSwaggerInfo.ContactUrl)
                },

                Extensions = infoExtension
            };

            return info;
        }

        private static ApiVersion ToApiVersion(string version)
        {
            var values = version.ToLower(CultureInfo.InvariantCulture).Replace("v", string.Empty).Split(".").Select(number => number.ToInt()).ToArray();
            if (values.Length == 1)
            {
                return new ApiVersion(values.First(), 0);
            }

            if (values.Length == 2)
            {
                return new ApiVersion(values.First(), values.ElementAt(1));
            }

            throw new ProblemDetailsException(500, "Unknown version string", $"Could not convert swagger version info: '{version}' into AP-Version");
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
