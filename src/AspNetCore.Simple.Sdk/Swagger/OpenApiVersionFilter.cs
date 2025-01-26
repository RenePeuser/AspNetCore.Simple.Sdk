using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
   public class OpenApiVersionFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Extensions["swagger"] = new OpenApiString("2.0");
    }
}
}
