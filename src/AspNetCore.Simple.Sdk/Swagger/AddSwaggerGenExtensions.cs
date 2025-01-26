using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public static class AddSwaggerGenExtensions
    {
        public static void AddSwaggerGenSimplified(this IServiceCollection services,
                                                   Assembly assembly,
                                                   IConfiguration configuration,
                                                   ILogger logger)
        {
            var swaggerInfos = configuration.GetSetting<SwaggerInfos>() ?? Swagger.GetDefaultSwaggerInfos(logger, new ApiVersion(1, 0));

            services.AddSingletonIfNotExists(swaggerInfos);

            var apiVersionProvider = new ApiVersionProvider();
            var allApiVersions = apiVersionProvider.GetAllApiVersions(assembly);

            services.AddSwaggerGen(options =>
            {
                options.DocInclusionPredicate((_, _) => true);
                options.AddSwaggerGrouping();
                options.SupportNonNullableReferenceTypes();

                // This have to come first !!
                options.OperationFilter<SetSelectedDocumentOperationFilter>();
                options.OperationFilter<RemoveVersionParameterFilter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
                options.DocumentFilter<AdditionalPropertiesFilter>();
                options.DocumentFilter<RootLevelTagsFilter>();
                options.DocumentFilter<SchemaFilterForCurrentVersion>();
                options.DocumentFilter<OpenApiVersionFilter>();

                options.ResolveConflictingActions(apiDescriptions =>
                {
                    var exactApiDescription = apiDescriptions.FirstOrDefault(apiDescription =>
                    {
                        var apiVersionAttribute = apiDescription.ActionDescriptor.EndpointMetadata.OfType<ApiVersionAttribute>().ToImmutableList();
                        if (apiVersionAttribute.IsEmpty())
                        {
                            return false;
                        }

                        if (apiVersionAttribute.Count > 1)
                        {
                            return false;
                        }

                        return apiVersionAttribute[0].Versions[0] == SwaggerUi.SelectedVersion;
                    });

                    var invalidDescription = apiDescriptions.First();

                    if (exactApiDescription.IsNull())
                    {
                        logger.LogError(@$"Swagger path could not be identified. 
Please check that your version for your documents are still available, do not delete older versions.
Current selected swagger version: '{SwaggerUi.SelectedVersion}'
{apiDescriptions.Select(desc => $"- {desc.HttpMethod} {desc.RelativePath}").Flatten(Environment.NewLine)}");

                        // so dirty, if it was refreshed we reset the list so evil
                        if (SwaggerUi.InvalidApiDescriptions.Contains(invalidDescription))
                        {
                            SwaggerUi.InvalidApiDescriptions.Clear();
                        }

                        SwaggerUi.InvalidApiDescriptions.Add(invalidDescription);
                    }

                    return exactApiDescription.IsNull() ? invalidDescription : exactApiDescription;

                    // return exactApiDescription;
                });

                options.AddBearerSecurityDefinition();
                options.AddBearerSecurityRequirement();


                options.AddXmlComments(assembly);
                options.CustomSchemaIds(type => type.ToString());

                // EnumSchemaFilter conflicts with ExtensibleEnumFilter. Use one or the other.
                // options.SchemaFilter<EnumSchemaFilter>();
                options.SchemaFilter<ExtensibleEnumFilter>();
                options.ParameterFilter<ExtensibleEnumFilter>();
                options.EnableAnnotations(); // necessary to include the SwaggerOperationAttribute.OperationIds in the Swagger Json

                foreach (var apiVersion in allApiVersions)
                {
                    var openApiInfo = GetVersionSpecificApiInfo(swaggerInfos, apiVersion, logger);

                    options.SwaggerDoc($"v{apiVersion.MajorVersion}.{apiVersion.MinorVersion}", openApiInfo);
                }

                options.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date", Example = new OpenApiString("2023-11-17") });
            });
        }

        private static OpenApiInfo GetVersionSpecificApiInfo(SwaggerInfos swaggerInfos, ApiVersion apiVersion, ILogger logger)
        {
            var versionSpecificSwaggerInfo = swaggerInfos.SwaggerInfosByVersion.FirstOrDefault(swagger =>
            {
                var apiVersionSwaggerInfo = swagger.Version.ToApiVersion();
                return apiVersionSwaggerInfo == apiVersion;
            });

            if (versionSpecificSwaggerInfo.IsNull())
            {
                var defaultSwaggerInfos = Swagger.GetDefaultSwaggerInfos(logger, apiVersion);
                versionSpecificSwaggerInfo = defaultSwaggerInfos.SwaggerInfosByVersion.First();
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

    internal static class Swagger
    {
        internal static SwaggerInfos GetDefaultSwaggerInfos(ILogger logger, ApiVersion apiVersion)
        {
            // Fallback no infos if nothing was found
            logger.LogError($"No specific swagger info was found for api version: {apiVersion}. Please check your appsettings.json, environment variables for a correct declaration to get swagger infos per version");
            var swaggerInfo = new SwaggerInfo
            {
                Audience = "company-internal",
                ContactEmail = "max.mustermann@hotmail.de",
                ContactName = "Max Mustermann",
                ContactUrl = "https://www.google.de",
                Description = "This is a very cool API V1",
                Id = Guid.NewGuid(),
                Title = "Cool API V1",
                Version = "1"
            };

            var swaggerInfos = new SwaggerInfos { SwaggerInfosByVersion = [swaggerInfo] };

            var message = $"{JToken.Parse(swaggerInfos.ToJson()).ToString(Formatting.Indented)}";
            logger.LogInformation(message);

            return swaggerInfos;
        }
    }
}
