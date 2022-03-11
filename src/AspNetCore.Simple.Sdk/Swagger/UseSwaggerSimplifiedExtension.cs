using System.Collections.Generic;
using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public static class UseSwaggerSimplifiedExtension
    {
        public static void UseSwaggerSimplified(this IApplicationBuilder app, string basePath)
        {
            var routeTemplate = basePath.IsNullOrWhiteSpace() ?
                $"/swagger/{{documentName}}/swagger.json" :
                $"{basePath}/swagger/{{documentName}}/swagger.json";

            app.UseSwagger(c =>
            {

                c.RouteTemplate = routeTemplate;
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var httpScheme = httpReq.Scheme;
#if (!DEBUG)
                    httpScheme = "https";
#endif
                    swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpScheme}://{httpReq.Host.Value}{basePath}" } };
                });
            });
        }
    }
}
