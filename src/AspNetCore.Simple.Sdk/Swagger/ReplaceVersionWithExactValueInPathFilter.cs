using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var collectPathInfos = CollectInfos(swaggerDoc, context).Distinct(item => item.key).ToList();
            var newPath = new OpenApiPaths();
            collectPathInfos.ForEach(path => newPath.Add(path.key, path.openApiPathItem));
            swaggerDoc.Paths = newPath;
        }


        private IEnumerable<(string key, OpenApiPathItem openApiPathItem)> CollectInfos(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var path in swaggerDoc.Paths)
            {
                var apiDescriptions = context.ApiDescriptions.Where(api => api.RelativePath.EqualsTo(path.Key.TrimStart('/'))).ToList();

                foreach (var apiDescription in apiDescriptions)
                {
                    var versionInfo = apiDescription.ActionDescriptor.EndpointMetadata.FirstOrDefaultOfType<ApiVersionAttribute>();


                    var newOpenApiPathItem = path.Value;
                    if (versionInfo.IsNull())
                    {
                        yield return (path.Key.Replace("{version}", swaggerDoc.Info.Version), newOpenApiPathItem);
                        continue;
                    }

                    var apiVersion = Convert(swaggerDoc.Info);
                    if (versionInfo.Versions.Any(v => v.EqualsTo(apiVersion)))
                    {
                        yield return (path.Key.Replace("{version}", swaggerDoc.Info.Version), newOpenApiPathItem);
                    }
                }
            }
        }

        private ApiVersion Convert(OpenApiInfo openApiInfo)
        {
            var values = openApiInfo.Version.ToLower(CultureInfo.InvariantCulture).Replace("v", string.Empty).Split(".").Select(number => number.ToInt()).ToArray();
            if (values.Length == 1)
            {
                return new ApiVersion(values.First(), 0);
            }

            if (values.Length == 2)
            {
                return new ApiVersion(values.First(), values.ElementAt(1));
            }

            throw new ProblemDetailsException(500, "Unknown version string", $"Could not convert swagger version info: '{openApiInfo.Version}' into AP-Version");
        }
    }
}
