using System.Collections.Immutable;
using System.Linq;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    /// <summary>Ensures that each tag defined on the operation level also exists on the root level of the OpenAPI Json document.</summary>
    public class RootLevelTagsFilter : IDocumentFilter
    {
        private readonly SwaggerInfos _swaggerInfos;

        public RootLevelTagsFilter(SwaggerInfos swaggerInfos)
        {
            _swaggerInfos = swaggerInfos;
        }

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

        private ImmutableArray<OpenApiTag> GetAllOpenApiTags(ImmutableList<ApiDescription> apiDescriptionsVersionBased, ImmutableArray<string> existingDocTagNames)
        {
            return apiDescriptionsVersionBased.SelectMany(desc => desc.ActionDescriptor.EndpointMetadata)
                                              .OfType<SwaggerOperationAttribute>()
                                              .SelectMany(op => op.Tags)
                                              .Distinct()
                                              .Where(optTag => optTag.IsNotNullOrWhiteSpace())
                                              .Except(existingDocTagNames)
                                              .Select(missingDocTagName => new OpenApiTag { Name = missingDocTagName })
                                              .ToImmutableArray();
        }

        private ImmutableList<ApiDescription> GetApiDescriptionsForSelectedVersion(DocumentFilterContext context, ApiVersion selectedVersion)
        {
            return context.ApiDescriptions
                          .Where(desc =>
                          {

                              var controllerActionDescriptor = desc.ActionDescriptor.As<ControllerActionDescriptor>();
                              if (controllerActionDescriptor.IsNull())
                              {
                                  return false;
                              }

                              if (_swaggerInfos.IncludeOnlyVersionedPaths.IsFalse())
                              {
                                  return true;
                              }

                              var apiVersionAttribute = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<ApiVersionAttribute>();
                              if (apiVersionAttribute.IsNull())
                              {
                                  return false;
                              }

                              // ToDo: Workaround, if no version is set V1 and no Version means the same for tags
                              return apiVersionAttribute.Versions.Any(version => version == selectedVersion);
                          }).ToImmutableList();
        }
    }
}
