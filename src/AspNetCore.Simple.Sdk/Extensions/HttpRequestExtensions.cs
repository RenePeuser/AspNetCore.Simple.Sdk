using System.Collections.Generic;
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

        public static IEnumerable<(string key, object value)> GetQueryRequestInfo(this HttpRequest httpRequest)
        {
            yield return ("Request", httpRequest.GetDisplayUrl());

            foreach (var header in httpRequest.Headers)
            {
                if (header.Key == HeaderNames.Authorization)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(header.Value.First().Split().Last());

                    yield return ("Authorization", token.Payload.ToDictionary(item => item.Key, item => item.Value));
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

        public static string GetReferer(this HttpRequest source)
        {
            return GetFirstHeaderValueOrDefault(source, HeaderNames.Referer);
        }

        public static string GetCloudFrontId(this HttpRequest httpRequest)
        {
            return GetFirstHeaderValueOrDefault(httpRequest, "X-Amz-Cf-Id", "n.A");
        }

        public static string GetFirstHeaderValueOrDefault(this HttpRequest httpRequest, string headerKey, string defaultValue = "")
        {
            var headerValues = httpRequest?.Headers?.GetValueOrDefault(headerKey);
            if (headerValues.HasValue.IsFalse())
            {
                return defaultValue;
            }

            var headerValuesValue = headerValues.Value.FirstOrDefault();
            if (headerValuesValue.IsNull())
            {
                return defaultValue;
            }

            return headerValuesValue;
        }
    }
}