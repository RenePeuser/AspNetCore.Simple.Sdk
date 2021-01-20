using Microsoft.Extensions.Configuration;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T GetSetting<T>(this IConfiguration configuration)
        {
            return configuration.GetSection(typeof(T).Name).Get<T>();
        }

        public static T GetSetting<T>(this IConfiguration configuration, string sectionName)
        {
            return configuration.GetSection(sectionName).Get<T>();
        }
    }
}
