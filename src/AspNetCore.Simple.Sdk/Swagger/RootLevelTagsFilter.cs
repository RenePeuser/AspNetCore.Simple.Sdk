using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    /// <summary>Ensures that each tag defined on the operation level also exists on the root level of the OpenAPI Json document.</summary>
    public class RootLevelTagsFilter(SwaggerInfos swaggerInfos) : IDocumentFilter
    {
        void IDocumentFilter.Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var selectedVersion = swaggerDoc.Info.Version.ToApiVersion();
            var existingDocTagNames = swaggerDoc.Tags.Select(docTag => docTag.Name).ToImmutableArray();

            // 1. Filter for current selected version all api endpoints out
            var apiDescriptionsVersionBased = GetApiDescriptionsForSelectedVersion(context, selectedVersion);

            // 2. Collect all swagger open api tags
            var missingDocTags = GetAllOpenApiTags(apiDescriptionsVersionBased, existingDocTagNames);

            // 3. Add all missing tags to the swagger doc.
            swaggerDoc.Tags.AddRange(missingDocTags);
        }

        private ImmutableList<OpenApiTag> GetAllOpenApiTags(ImmutableList<ApiDescription> apiDescriptionsVersionBased, ImmutableArray<string> existingDocTagNames)
        {
            var controllerActionDescriptor = apiDescriptionsVersionBased.Select(apiDescription => apiDescription.ActionDescriptor).OfType<ControllerActionDescriptor>().ToImmutableList();
            var swaggerOperationAttributes = controllerActionDescriptor.SelectMany(descriptor => descriptor.EndpointMetadata).OfType<SwaggerOperationAttribute>().ToImmutableList();
            var tags = swaggerOperationAttributes.Where(attribute => attribute.Tags.IsNotNull()).SelectMany(swaggerOperation => swaggerOperation.Tags).Distinct().ToImmutableList();

            var openApiTags = tags.Where(optTag => optTag.IsNotNullOrWhiteSpace())
                                  .Except(existingDocTagNames)
                                  .Select(missingDocTagName => new OpenApiTag { Name = missingDocTagName })
                                  .ToImmutableList();

            return openApiTags;
        }

        private ImmutableList<ApiDescription> GetApiDescriptionsForSelectedVersion(DocumentFilterContext context, ApiVersion selectedVersion)
        {
            return context.ApiDescriptions
                          .Where(desc =>
                          {
                              // 1. Must be an controller action descriptor otherwise no checks can be done
                              var controllerActionDescriptor = desc.ActionDescriptor.As<ControllerActionDescriptor>();
                              if (controllerActionDescriptor.IsNull())
                              {
                                  return false;
                              }

                              // 2. If all versions and non versions allowed we return true to show all
                              if (swaggerInfos.IncludeOnlyVersionedPaths.IsFalse())
                              {
                                  return true;
                              }

                              // Since here only exact version matched are allowed

                              // 3. Detect all path exists on the controller to check path to ignore
                              var allPaths = GetAllPaths(controllerActionDescriptor).ToImmutableList();
                              if (allPaths.All(path => path.Contains("{version:apiVersion}").IsFalse()))
                              {
                                  return false;
                              }

                              // 4. Now we have to check if version attribute exi
                              var apiVersionAttribute = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<ApiVersionAttribute>();
                              var apiVersionNeutral = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<ApiVersionNeutralAttribute>();

                              // 5. If neutral api version then it will shown in all version documents
                              if (apiVersionAttribute.IsNull() && apiVersionNeutral.IsNotNull())
                              {
                                  return true;
                              }

                              // 6. Now we check if version match selected version document
                              if (apiVersionAttribute.IsNotNull())
                              {
                                  // 4. Then we have to check if the current selected version fits the controller, because we only want to show version
                                  //    specific controller, routes, tags and schemas
                                  if (apiVersionAttribute.Versions.Any(version => version == selectedVersion).IsFalse())
                                  {
                                      return false;
                                  }
                              }

                              return true;
                          }).ToImmutableList();
        }

        private IEnumerable<string> GetAllPaths(ControllerActionDescriptor controllerActionDescriptor)
        {
            var controllerBasePaths = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes<RouteAttribute>();
            foreach (var controllerBasePath in controllerBasePaths)
            {
                yield return controllerBasePath.Template;
            }

            var controllerMethods = controllerActionDescriptor.ControllerTypeInfo.GetMethods();
            var methodRouteAttributes = controllerMethods.SelectMany(method => method.GetCustomAttributes<RouteAttribute>());
            foreach (var methodRouteAttribute in methodRouteAttributes)
            {
                yield return methodRouteAttribute.Template;
            }

            var httpAttributes = controllerMethods.SelectMany(method => method.GetCustomAttributes<HttpMethodAttribute>());
            foreach (var httpMethodAttribute in httpAttributes)
            {
                if (httpMethodAttribute.Template.IsNotNull())
                {
                    yield return httpMethodAttribute.Template;
                }
            }
        }
    }
}
