using System;
using System.Linq;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class StringExtensions
    {
        public static Uri ToUri(this string source)
        {
            return new Uri(source);
        }

        public static string BuildUriPathWith(this string basePath, params string[] pathSegments)
        {
            var normalizeBasPath = basePath.TrimEnd('/');
            var normalizePathSegments = pathSegments.Select(segment => segment.TrimStart('/'));

            return normalizeBasPath.Concat(normalizePathSegments).Flatten("/");
        }
    }
}