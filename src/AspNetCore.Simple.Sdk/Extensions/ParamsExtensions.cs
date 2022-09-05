using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class ParamsExtensions
    {
        public static Dictionary<string, string> ToDictionary(this (string key, string? value)[] details)
        {
            var dictionary = details.ToDictionary(item => item.key, item => item.value?.ToString() ?? "null");
            return dictionary;
        }
    }
}
