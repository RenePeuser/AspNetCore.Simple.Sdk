using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Mediator;
using AspNetCore.Simple.Sdk.Startups;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace AspNetCore.Simple.Sdk.Swagger
{
    /// <summary>
    /// This query provides all Swagger documents in a c# object oriented structure to work with.
    /// </summary>
    /// <param name="Startup">The type of your startup.cs</param>
    /// <param name="Client">The Http client which was setup for the test environment.</param>
    public sealed record GetAllSwaggerVersionDocuments(Type Startup, HttpClient Client) : IQuery<IImmutableList<OpenApiDocument>>;

    internal sealed class GetAllSwaggerVersionDocumentsHandler(IApiVersionProvider apiVersionProvider,
                                                               BasePath path) : IQueryHandler<GetAllSwaggerVersionDocuments, IImmutableList<OpenApiDocument>>
    {
        public async Task<IImmutableList<OpenApiDocument>> Handle(GetAllSwaggerVersionDocuments request, CancellationToken cancellationToken)
        {
            var apiVersions = apiVersionProvider.GetAllApiVersions(request.Startup.Assembly).ToImmutableList();

            var apiVersionInfos = await FetchAllSwaggerDocuments(apiVersions).ToImmutableListAsync(cancellationToken).ConfigureAwait(false);

            return apiVersionInfos;

            async IAsyncEnumerable<OpenApiDocument> FetchAllSwaggerDocuments(IImmutableList<ApiVersion> apiVersions)
            {
                foreach (var apiVersion in apiVersions)
                {
                    var basePath = path.Value.HasValue ? $"{path.Value.Value}/" : string.Empty;
                    var swaggerResponse = await request.Client.GetAsync($"{basePath}swagger/v{apiVersion.MajorVersion}.{apiVersion.MinorVersion}/swagger.json", cancellationToken).ConfigureAwait(false);
                    if (swaggerResponse.IsSuccessStatusCode.IsFalse())
                    {
                        throw new ProblemDetailsException("Fetching swagger json was not successful, please check console error output for existing errors");
                    }

                    var swaggerJson = await swaggerResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                    var openApiDocument = new OpenApiStreamReader().Read(swaggerJson, out _);

                    yield return openApiDocument;
                }
            }
        }
    }
}
