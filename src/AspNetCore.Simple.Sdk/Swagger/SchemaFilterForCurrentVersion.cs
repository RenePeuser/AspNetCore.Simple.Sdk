using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Extensions.Pack;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public partial class SchemaFilterForCurrentVersion : IDocumentFilter
    {
        private static readonly Regex VersionRegex = GetVersionRegex();
        private readonly Assembly _callingAssembly;

        public SchemaFilterForCurrentVersion(Assembly callingAssembly)
        {
            _callingAssembly = callingAssembly;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var assemblyRootName = _callingAssembly.GetName().Name!.ToUpperInvariant()!;

            var currentVersionSpecificSchema = swaggerDoc.Components.Schemas.Where(keyValue =>
            {
                var key = keyValue.Key.ToUpperInvariant();

                // If not caller owned namespace we accept all versions
                if (key.Contains(assemblyRootName).IsFalse())
                {
                    return true;
                }

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

        [GeneratedRegex("([V])\\d")]
        private static partial Regex GetVersionRegex();
    }
}
