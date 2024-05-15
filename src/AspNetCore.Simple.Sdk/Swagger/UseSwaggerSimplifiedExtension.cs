using System.Collections.Generic;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public static class UseSwaggerSimplifiedExtension
    {
        public static void UseSwaggerSimplified(this IApplicationBuilder app,
                                                IConfiguration configuration,
                                                string basePath,
                                                ILogger logger)
        {
            var swaggerInfo = configuration.GetSetting<SwaggerInfos>() ?? Swagger.GetDefaultSwaggerInfos(logger, new ApiVersion(1, 0));

            var routeTemplate = basePath.IsNullOrWhiteSpace() ? $"/swagger/{{documentName}}/swagger.json" : $"{basePath}/swagger/{{documentName}}/swagger.json";

            app.UseSwagger(c =>
            {
                c.RouteTemplate = routeTemplate;

                if (swaggerInfo.WithServerInfo)
                {
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        var httpScheme = httpReq.Scheme;
#if !DEBUG
                    httpScheme = "https";
#endif
                        swaggerDoc.Servers = new List<OpenApiServer> { new() { Url = $"{httpScheme}://{httpReq.Host.Value}{basePath}" } };
                    });
                }
            });
        }
    }
}
