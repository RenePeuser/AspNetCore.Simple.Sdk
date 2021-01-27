using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public static class UseSwaggerSimplifiedExtension
    {
        public static void UseSwaggerSimplified(this IApplicationBuilder app, string basePath)
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = $"{basePath}/swagger/{{documentName}}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var httpScheme = httpReq.Scheme;
#if (!DEBUG)
                    httpScheme = "https";
#endif
                    swaggerDoc.Servers = new List<OpenApiServer> {new OpenApiServer {Url = $"{httpScheme}://{httpReq.Host.Value}{basePath}"}};
                });
            });
        }
    }
}
