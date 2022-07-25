using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class DefaultValidator : SettingsValidator<DefaultValidator>
    {
        public override void Validate(DefaultValidator setting)
        {
            // Default validation, if no validator is given - nothing will be validated !
        }
    }

    public class NoCustomRegistration : ICustomTypeRegistration
    {
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            // Do nothing here - Null object pattern
        }
    }
}
