using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy
{
    [TestClass]
    public class Type_Registration_Tests
    {
        [TestMethod]
        public void Should_Register_Correct_Lifetime_Automatically_By_Type_With_All_Needed_Dependencies()
        {
            var configuration = new ConfigurationBuilder().Build();
            var serviceCollection = new ServiceCollection();
            var autoRegister = new AutoRegistrationFactory().Create(serviceCollection, configuration);

            autoRegister.DoAutoRegistrationFor(typeof(ScopedRootRootService));

            Assert.AreEqual(3, serviceCollection.Count);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<IScopedRootService>());
            Assert.IsNotNull(serviceProvider.GetService<ServiceHierarchy01>());
            Assert.IsNotNull(serviceProvider.GetService<ServiceHierarchy02>());
        }

        [TestMethod]
        public void Should_Register_Correct_Lifetime_Automatically_By_Generic_Type_With_All_Needed_Dependencies()
        {
            var configuration = new ConfigurationBuilder().Build();
            var serviceCollection = new ServiceCollection();
            var autoRegister = new AutoRegistrationFactory().Create(serviceCollection, configuration);

            autoRegister.DoAutoRegistrationFor<ScopedRootRootService>();

            Assert.AreEqual(3, serviceCollection.Count);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<IScopedRootService>());
            Assert.IsNotNull(serviceProvider.GetService<ServiceHierarchy01>());
            Assert.IsNotNull(serviceProvider.GetService<ServiceHierarchy02>());
        }
    }
}
