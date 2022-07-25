using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.AppSettings.Settings
{
    [AppSettingsRegistration("Settings", typeof(ScopedSettings), ServiceLifetime.Transient)]
    public class TransientSettings
    {
        public string Name { get; init; }
    }
}