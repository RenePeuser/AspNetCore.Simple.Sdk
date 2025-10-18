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
    public class Type_Registration_Tests
    {
        [TestMethod]
        [DataRow(typeof(ScopedService), ServiceLifetime.Scoped)]
        [DataRow(typeof(TransientService), ServiceLifetime.Transient)]
        [DataRow(typeof(SingletonService), ServiceLifetime.Singleton)]
        public void Should_Register_Correct_Lifetime_Automatically(Type type, ServiceLifetime serviceLifetime)
        {
            var configuration = new ConfigurationBuilder().Build();
            var serviceCollection = new ServiceCollection();
            var autoregister = new AutoRegistrationFactory().Create(serviceCollection, configuration);

            autoregister.DoAutoRegistrationFor(type);

            Assert.AreEqual(1, serviceCollection.Count);

            var serviceRegistration = serviceCollection.First();
            Assert.AreEqual(type, serviceRegistration.ServiceType);
            Assert.AreEqual(type, serviceRegistration.ImplementationType);
            Assert.AreEqual(serviceLifetime, serviceRegistration.Lifetime);
        }
    }
}
