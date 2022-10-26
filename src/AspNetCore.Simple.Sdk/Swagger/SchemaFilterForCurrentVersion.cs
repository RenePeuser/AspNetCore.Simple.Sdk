using System.Linq;
using System.Text.RegularExpressions;
using Extensions.Pack;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public class SchemaFilterForCurrentVersion : IDocumentFilter
    {
        private static readonly Regex VersionRegex = new("([V])\\d");

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var currentVersionSpecificSchema = swaggerDoc.Components.Schemas.Where(keyValue =>
            {
                var key = keyValue.Key.ToUpperInvariant();

                // No version return
                if (VersionRegex.Match(key).Success.IsFalse())
                {
                    return true;
                }

                // if path contains current document name == V1.0 (Version)
                if (key.Contains(context.DocumentName.ToUpperInvariant()))
                {
                    return true;
                }

                // normalize version v1
                var normalizedVersion = context.DocumentName.Split('.').FirstOrDefault();
                if (normalizedVersion.IsNullOrWhiteSpace())
                {
                    return false;
                }

                return key.Contains(normalizedVersion.ToUpperInvariant());
            }).ToList();


            swaggerDoc.Components.Schemas.ClearAndAddRange(currentVersionSpecificSchema);
        }
    }
}
