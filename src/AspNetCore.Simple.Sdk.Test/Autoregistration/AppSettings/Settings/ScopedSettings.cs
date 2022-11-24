using System;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.AppSettings.Settings
{
    [AppSettingsRegistration("Settings", typeof(ScopedSettings), ServiceLifetime.Scoped)]
    public class ScopedSettings
    {
        public string Name { get; init; } = string.Empty;
    }

    [AppSettingsRegistration("Settings", typeof(ScopedSettingsWithValidator), ServiceLifetime.Scoped, typeof(ScopeSettingsValidator))]
    public class ScopedSettingsWithValidator
    {
        public string Name { get; init; } = string.Empty;
    }

    [AppSettingsRegistration("Settings", typeof(ScopedSettingsWithValidator), ServiceLifetime.Scoped, typeof(ScopeSettingsValidator))]
    public class ScopedSettingsWithCustomRegistration
    {
        public string Name { get; init; } = string.Empty;
    }

    public class CustomRegistrationForScopedSettings : ICustomTypeRegistration
    {
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }

    public class ScopeSettingsValidator : SettingsValidator<ScopedSettingsWithValidator>
    {
        public override void Validate(ScopedSettingsWithValidator setting)
        {
            var expectedValue = "Son Goku";
            if (setting.Name.NotEqualsTo(expectedValue))
            {
                throw new ArgumentException($"The property: '{nameof(ScopedSettingsWithValidator.Name)}' does not match expected value: '{expectedValue}'");
            }
        }
    }

}
