using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.ConfigurationExtensions
{
    public abstract class ConfigurationTestBase
    {
        protected IConfigurationRoot Configuration { get; private set; } = null!;

        [TestInitialize]
        public void Setup()
        {
            // 1. Fetching secrets to get a connection string, for a test database
            var filePath = Path.Combine(Environment.CurrentDirectory, @"ConfigurationExtensions\appsettings.test.json");
            var fileInfo = new FileInfo(filePath);

            fileInfo.Exists.Should().BeTrue($"File: '{fileInfo.FullName}' does not exists");

            Configuration = new ConfigurationBuilder().AddJsonFile(fileInfo.FullName).Build();
        }
    }
}
