using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using MediatR;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace AspNetCore.Simple.Sdk.Swagger
{
    /// <summary>
    /// This query provides all Swagger documents in a c# object oriented structure to work with.
    /// </summary>
    /// <param name="Startup">The type of your startup.cs</param>
    public record GetAllSwaggerVersionDocuments(Type Startup, HttpClient Client) : IStreamRequest<OpenApiDocument>;

    internal class GetAllSwaggerVersionDocumentsHandler : IStreamRequestHandler<GetAllSwaggerVersionDocuments, OpenApiDocument>
    {
        private readonly IApiVersionProvider _apiVersionProvider;

        public GetAllSwaggerVersionDocumentsHandler(IApiVersionProvider apiVersionProvider)
        {
            _apiVersionProvider = apiVersionProvider;
        }

        public async IAsyncEnumerable<OpenApiDocument> Handle(GetAllSwaggerVersionDocuments request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var apiVersions = _apiVersionProvider.GetAllApiVersions(request.Startup.Assembly).ToImmutableList();

            foreach (var apiVersion in apiVersions)
            {
                var swaggerResponse = await request.Client.GetAsync($"swagger/v{apiVersion.MajorVersion}.{apiVersion.MinorVersion}/swagger.json", cancellationToken).ConfigureAwait(false);
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
