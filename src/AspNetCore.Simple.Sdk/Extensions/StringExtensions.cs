using System;
using System.IO;
using System.Linq;
using Extensions.Pack;

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
            var normalizeBasPath = basePath.TrimEnd(Path.AltDirectorySeparatorChar);
            var normalizePathSegments = pathSegments.Select(segment => segment.TrimStart(Path.AltDirectorySeparatorChar));

            return normalizeBasPath.Concat(normalizePathSegments).Flatten(Path.AltDirectorySeparatorChar.ToString());
        }
    }
}
