using System;
using System.Globalization;
using System.IO;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;

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

        public static ApiVersion ToApiVersion(this string version)
        {
            var values = version.ToLower(CultureInfo.InvariantCulture).Replace("v", string.Empty).Split(".").Select(number => number.ToInt()).ToArray();
            if (values.Length == 1)
            {
                return new ApiVersion(values.First(), 0);
            }

            if (values.Length == 2)
            {
                return new ApiVersion(values.First(), values.ElementAt(1));
            }

            throw new ProblemDetailsException(500, "Unknown version string", $"Could not convert swagger version info: '{version}' into AP-Version");
        }
    }
}
