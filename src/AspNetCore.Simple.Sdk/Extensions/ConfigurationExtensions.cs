using Microsoft.Extensions.Configuration;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T GetSetting<T>(this IConfiguration configuration)
        {
            return GetSetting<T>(configuration, typeof(T).Name);
        }

        public static T GetSetting<T>(this IConfiguration configuration, string settingName)
        {
            return configuration.GetSection(settingName).Get<T>();
        }
    }
}
