using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace AspNetCore.Simple.Sdk.Swagger
{
    public sealed record SwaggerUi
    {
        public static ApiVersion SelectedVersion { get; set; } = new(1, 0); //Exception cause swagger do not provide at specific scope the selected document

        public static List<ApiDescription> InvalidApiDescriptions { get; set; } = new();
    }
}
