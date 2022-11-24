using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.AppSettings.Settings
{
    [AppSettingsRegistration("Settings", typeof(ScopedSettings), ServiceLifetime.Singleton)]
    public class SingletonSettings
    {
        public string Name { get; init; } = string.Empty;
    }
}
