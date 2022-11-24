using System;
using System.Linq;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using AspNetCore.Simple.Sdk.Test.Autoregistration.OnlyImplementation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.OnlyImplementation
{
    [TestClass]
    public class Duplicate_Registration_Test
    {
        private IConfigurationRoot _configuration = null!;
        private ServiceCollection _serviceCollection = null!;
        private AutoRegistration _autoregister = null!;

        [TestInitialize]
        public void Init()
        {
            _configuration = new ConfigurationBuilder().Build();
            _serviceCollection = new ServiceCollection();
            _autoregister = new AutoRegistrationFactory().Create(_serviceCollection, _configuration);
        }

        [DataTestMethod]
        [DataRow(typeof(ScopedService), ServiceLifetime.Scoped)]
        [DataRow(typeof(TransientService), ServiceLifetime.Transient)]
        [DataRow(typeof(SingletonService), ServiceLifetime.Singleton)]
        public void Should_Not_Register_Types_Double_Which_Match_Same_Interface_And_Same_Implementation(Type type, ServiceLifetime serviceLifetime)
        {
            _autoregister.DoAutoRegistrationFor(type);
            _autoregister.DoAutoRegistrationFor(type);

            Assert.AreEqual(1, _serviceCollection.Count);

            var serviceRegistration = _serviceCollection.First();

            Assert.AreEqual(type, serviceRegistration.ServiceType);
            Assert.AreEqual(type, serviceRegistration.ImplementationType);
            Assert.AreEqual(serviceLifetime, serviceRegistration.Lifetime);
        }
    }
}
