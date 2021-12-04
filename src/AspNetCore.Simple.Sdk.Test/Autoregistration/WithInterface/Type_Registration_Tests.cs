using System;
using System.Linq;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface
{
    [TestClass]
    public class Type_Registration_Tests
    {
        [DataTestMethod]
        [DataRow(typeof(ScopedServiceWithInterface), typeof(IScopedService), ServiceLifetime.Scoped)]
        public void Should_Register_Correct_Lifetime_Automatically(Type implementationType, Type interfaceType, ServiceLifetime serviceLifetime)
        {
            var configuration = new ConfigurationBuilder().Build();
            var serviceCollection = new ServiceCollection();
            var autoRegister = new AutoRegistrationFactory().Create(serviceCollection, configuration);

            autoRegister.DoAutoRegistrationFor(implementationType);

            Assert.AreEqual(1, serviceCollection.Count);

            var serviceRegistration = serviceCollection.First();
            Assert.AreEqual(interfaceType, serviceRegistration.ServiceType);
            Assert.AreEqual(implementationType, serviceRegistration.ImplementationType);
            Assert.AreEqual(serviceLifetime, serviceRegistration.Lifetime);
        }

        [TestMethod]
        public void Should_Register_Correct_Lifetime_Automatically()
        {
            var configuration = new ConfigurationBuilder().Build();
            var serviceCollection = new ServiceCollection();
            var autoRegister = new AutoRegistrationFactory().Create(serviceCollection, configuration);

            autoRegister.DoAutoRegistrationFor<ScopedServiceWithInterface>();

            Assert.AreEqual(1, serviceCollection.Count);

            var serviceRegistration = serviceCollection.First();
            Assert.AreEqual(typeof(IScopedService), serviceRegistration.ServiceType);
            Assert.AreEqual(typeof(ScopedServiceWithInterface), serviceRegistration.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Scoped, serviceRegistration.Lifetime);
        }
    }
}
