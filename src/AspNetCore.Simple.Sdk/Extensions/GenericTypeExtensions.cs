using System.Collections.Generic;

namespace AspNetCore.Simple.Sdk.Extensions
{
    internal static class GenericTypeExtensions
    {
        internal static IEnumerable<T> ToEnumerable<T>(this T source)
        {
            yield return source;
        }
    }
}
