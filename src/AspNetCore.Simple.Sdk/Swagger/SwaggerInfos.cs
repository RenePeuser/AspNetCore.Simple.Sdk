using System;
using AspNetCore.Simple.Sdk.Extensions;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public static class AddSwaggerInfosExtension
    {
        public static void AddSwaggerInfos(this IServiceCollection services, IConfiguration configuration)
        {
            var swaggerInfos = configuration.GetSetting<SwaggerInfos>() ?? new SwaggerInfos();

            services.AddSingletonIfNotExists(swaggerInfos);
        }
    }

    public record SwaggerInfos
    {
        /// <summary>
        /// You can define swagger document infos per version you have. For each version use on <see cref="SwaggerInfo"/>
        /// </summary>
        public SwaggerInfo[] SwaggerInfosByVersion { get; init; } = Array.Empty<SwaggerInfo>();

        /// <summary>
        /// Controls that only path´s with a version are included version.
        /// </summary>
        public bool IncludeOnlyVersionedPaths { get; init; }

        public bool WithServerInfo { get; init; } = true;
    }
}
