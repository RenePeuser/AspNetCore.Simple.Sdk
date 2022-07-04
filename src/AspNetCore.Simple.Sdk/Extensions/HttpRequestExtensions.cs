using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class HttpRequestExtensions
    {
        public static void AddAuthorization(this HttpRequest request, string authorization)
        {
            request.Headers.Add(HeaderNames.Authorization, authorization);
        }

        public static IEnumerable<(string key, string value)> GetQueryRequestInfo(this HttpRequest httpRequest)
        {
            yield return ("Request", httpRequest.GetDisplayUrl());

            foreach (var header in httpRequest.Headers)
            {
                if (header.Key == HeaderNames.Authorization)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(header.Value.First().Split().Last());

                    yield return (HeaderNames.Authorization, token.Payload.ToDictionary(item => item.Key, item => item.Value).ToJson());
                }
                else
                {
                    yield return (header.Key, header.Value.Flatten(","));
                }
            }
        }

        public static string GetAuthorization(this HttpRequest source)
        {
            return GetFirstHeaderValueOrDefault(source, HeaderNames.Authorization);
        }

        public static string GetFirstHeaderValueOrDefault(this HttpRequest httpRequest, string headerKey, string defaultValue = "")
        {
            var headerValues = httpRequest.Headers.GetValueOrDefault(headerKey);
            if (headerValues.IsEmpty())
            {
                return defaultValue;
            }

            var headerValuesValue = headerValues.FirstOrDefault();
            if (headerValuesValue is null)
            {
                return defaultValue;
            }

            return headerValuesValue;
        }
    }
}
