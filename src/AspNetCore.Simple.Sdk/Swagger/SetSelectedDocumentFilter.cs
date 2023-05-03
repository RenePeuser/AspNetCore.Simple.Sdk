using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public class SetSelectedDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            SwaggerUi.SelectedVersion = context.DocumentName.ToApiVersion();
        }
    }
}
