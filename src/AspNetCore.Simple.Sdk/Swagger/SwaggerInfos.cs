using System;

namespace AspNetCore.Simple.Sdk.Swagger
{
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

        public bool WithServerInfo { get; init; }
    }
}
