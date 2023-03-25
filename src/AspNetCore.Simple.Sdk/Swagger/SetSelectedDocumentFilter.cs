using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public class SetSelectedDocumentFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            SwaggerUi.SelectedVersion = context.DocumentName.ToApiVersion();
        }
    }
}
