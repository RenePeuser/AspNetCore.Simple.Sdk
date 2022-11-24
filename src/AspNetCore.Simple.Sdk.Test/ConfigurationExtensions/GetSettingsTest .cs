using AspNetCore.Simple.MsTest.Sdk;
using AspNetCore.Simple.Sdk.Extensions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.ConfigurationExtensions
{
    public record Dummy
    {
        public string Value0 { get; init; } = string.Empty;

        public string Value1 { get; init; } = string.Empty;

        public string Value2 { get; init; } = string.Empty;
    }

    public record DummySettings : Dummy;

    public record Person;

    [TestClass]
    public class GetSettingsTest : ConfigurationTestBase
    {
        [TestMethod]
        public void Should_Be_Able_To_Fetch_Settings_Directly_By_Typename_Without_Settings_Postfix()
        {
            var settings = Configuration.GetSettings<Dummy>();

            settings.Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Be_Able_To_Fetch_Settings_Directly_By_Typename_With_Settings_Postfix()
        {
            var settings = Configuration.GetSettings<DummySettings>();

            settings.Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Be_Able_To_Fetch_Settings_Directly_By_Given_String_Path()
        {
            var settings = Configuration.GetSettings<DummySettings>(nameof(Dummy));

            settings.Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Be_Able_To_Fetch_Settings_With_Correct_Content()
        {
            var settings = Configuration.GetSettings<DummySettings>(nameof(Dummy));
            var expectedResult = new DummySettings { Value0 = "A", Value1 = "B", Value2 = "C" };

            Assert.That.ObjectsAreEqual(() => settings, () => expectedResult);
        }

        [TestMethod]
        public void Should_Throw_Missing_Settings_Exception_When_Settings_Does_Not_Exists()
        {
            var exception = Assert.ThrowsException<MissingSettingsException<Person>>(Configuration.GetSettings<Person>);

            exception?.Message.Should().Be($"The setting: '{nameof(Person)}' is missing. Please check your specific appsettings.json or your environment variables.");
        }
    }
}
