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
        [TestMethod]
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

        [TestMethod]
        [DataRow(typeof(ScopedServiceWithInterfaceDependencies), ServiceLifetime.Scoped)]
        public void Should_Register_Correct_Lifetime_If_Dependency_Is_An_Interface_Automatically(Type implementationType, ServiceLifetime serviceLifetime)
        {
            var configuration = new ConfigurationBuilder().Build();
            var serviceCollection = new ServiceCollection();
            var autoRegister = new AutoRegistrationFactory().Create(serviceCollection, configuration);

            autoRegister.DoAutoRegistrationFor(implementationType);

            Assert.AreEqual(2, serviceCollection.Count);

            var implementationTypeRegistration = serviceCollection.FirstOrDefault(registration => registration.ImplementationType == implementationType);
            Assert.IsNotNull(implementationTypeRegistration);
            Assert.AreEqual(serviceLifetime, implementationTypeRegistration.Lifetime);

            var dependencyTypeRegistration = serviceCollection.FirstOrDefault(registration => registration.ServiceType == typeof(IScopedService));
            Assert.IsNotNull(dependencyTypeRegistration);
            Assert.AreEqual(serviceLifetime, dependencyTypeRegistration.Lifetime);
        }

        [Ignore]
        [TestMethod]
        [DataRow(typeof(ScopedServiceWithListOfInterfaceDependencies), ServiceLifetime.Scoped)]
        public void Should_Register_All_Dependencies_If_They_Are_Injected_By_EnumerationTypes(Type implementationType, ServiceLifetime serviceLifetime)
        {
            var configuration = new ConfigurationBuilder().Build();
            var serviceCollection = new ServiceCollection();
            var autoRegister = new AutoRegistrationFactory().Create(serviceCollection, configuration);

            autoRegister.DoAutoRegistrationFor(implementationType);

            Assert.AreEqual(3, serviceCollection.Count);

            var implementationTypeRegistration = serviceCollection.FirstOrDefault(registration => registration.ImplementationType == implementationType);
            Assert.IsNotNull(implementationTypeRegistration);
            Assert.AreEqual(serviceLifetime, implementationTypeRegistration.Lifetime);

            var dependencyTypeRegistration1 = serviceCollection.FirstOrDefault(registration => registration.ImplementationType == typeof(Strategy1));
            Assert.IsNotNull(dependencyTypeRegistration1);
            Assert.AreEqual(serviceLifetime, dependencyTypeRegistration1.Lifetime);

            var dependencyTypeRegistration2 = serviceCollection.FirstOrDefault(registration => registration.ImplementationType == typeof(Strategy2));
            Assert.IsNotNull(dependencyTypeRegistration2);
            Assert.AreEqual(serviceLifetime, dependencyTypeRegistration2.Lifetime);
        }
    }
}
