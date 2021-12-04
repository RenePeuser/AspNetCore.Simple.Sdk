using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class AutoRegistrationFactory
    {
        public AutoRegistration Create(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var lifetimeDetector = new LifetimeDetector();
            var appsettingsRegistrationStrategy = new AppSettingsRegistrationStrategy(lifetimeDetector, serviceCollection, configuration);
            var interfaceDetector = new InterfaceDetector();
            var serviceRegistrationStrategy = new ServiceRegistrationStrategy(serviceCollection, lifetimeDetector, interfaceDetector);
            var registrationCheck = new RegistrationCheck(serviceCollection);
            var dependencyDetector = new DependencyDetector();
            var registrationStrategy = new TypeRegistration(new IRegistrationStrategy[] { appsettingsRegistrationStrategy, serviceRegistrationStrategy }, registrationCheck, dependencyDetector);
            var autoRegistration = new AutoRegistration(registrationStrategy);
            return autoRegistration;
        }
    }
}
