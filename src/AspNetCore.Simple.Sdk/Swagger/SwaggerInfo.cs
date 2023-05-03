using System;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public record SwaggerInfo
    {
        /// <summary>See Zalando open source <a href="https://opensource.zalando.com/restful-api-guidelines/#215"> guideline 215</a>.</summary>
        public Guid? Id { get; init; }

        /// <summary>See Zalando open source <a href="https://opensource.zalando.com/restful-api-guidelines/#219"> guideline 219</a>.</summary>
        public string Audience { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string ContactName { get; init; } = string.Empty;

        public string ContactEmail { get; init; } = string.Empty;

        public string? ContactUrl { get; init; }

        public string Version { get; init; } = "1.0";
    }
}
