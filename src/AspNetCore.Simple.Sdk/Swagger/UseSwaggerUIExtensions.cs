using System.Reflection;
using AspNetCore.Simple.Sdk.ApiVersioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public static class UseSwaggerUiSimplifiedExtension
    {
        public static void UseSwaggerUiSimplified(this IApplicationBuilder app, Assembly assemblies, PathString pathString)
        {
            app.UseSwaggerUI(c =>
            {
                var trimmedPath = pathString.Value.TrimStart('/');
                c.RoutePrefix = $"{trimmedPath}/swagger".TrimStart('/');
                var allApiVersions = new ApiVersionProvider().GetAllApiVersions(assemblies);
                foreach (var apiVersion in allApiVersions)
                {
                    c.SwaggerEndpoint($"/{trimmedPath}/swagger/v{apiVersion.MajorVersion}.{apiVersion.MinorVersion}/swagger.json", $"V{apiVersion.MajorVersion}.{apiVersion.MinorVersion}");
                }
            });
        }
    }
}
