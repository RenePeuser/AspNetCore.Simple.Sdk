using System;
using System.Net.Http;
using AspNetCore.Simple.MsTest.Sdk;
using AspNetCore.Simple.Sdk.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test
{
    [TestClass]
    public abstract class MsTestBase
    {
        // In this sample we currently use assembley intialize, to save performance, but you can do it also different.
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext _)
        {
            // Create this with new, is not a fault, the reason is to keep the test class more cleaner.
            ApiTestBase = new ApiTestBase<Startup>("Development", (collection, configuration) => { });
            ServiceProvider = ApiTestBase.Services;
            Client = ApiTestBase.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        protected static ApiTestBase<Startup> ApiTestBase { get; set; } = null!;

        protected static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected static HttpClient Client { get; private set; } = null!;

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            ApiTestBase.Dispose();
            Client.Dispose();
        }
    }
}
