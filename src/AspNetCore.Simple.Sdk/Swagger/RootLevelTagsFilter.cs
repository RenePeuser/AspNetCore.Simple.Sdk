using System.Collections.Immutable;
using System.Linq;
using Extensions.Pack;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    /// <summary>Ensures that each tag defined on the operation level also exists on the root level of the OpenAPI Json document.</summary>
    public class RootLevelTagsFilter : IDocumentFilter
    {
        void IDocumentFilter.Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var existingDocTagNames = swaggerDoc.Tags.Select(docTag => docTag.Name).ToImmutableArray();
            var missingDocTags = context
                .ApiDescriptions
                .SelectMany(desc => desc.ActionDescriptor.EndpointMetadata)
                .OfType<SwaggerOperationAttribute>()
                .SelectMany(op => op.Tags)
                .Distinct()
                .Where(optTag => optTag.IsNotNullOrWhiteSpace())
                .Except(existingDocTagNames)
                .Select(missingDocTagName => new OpenApiTag { Name = missingDocTagName })
                .ToImmutableArray();
            swaggerDoc.Tags.AddRange(missingDocTags);
        }
    }
}
