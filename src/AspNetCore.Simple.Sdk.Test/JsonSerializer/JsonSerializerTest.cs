using System.Security;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.JsonSerializer
{
    [TestClass]
    [TestCategory("IJsonSerializer")]
    public class JsonSerializerTest : MsTestBase
    {
        [TestMethod]
        public void Security_Critial_Class_Have_Not_To_Be_Print_Out_In_Error_Cases()
        {
            var serializer = ServiceProvider.GetRequiredService<IJsonSerializer>();
            var criticalJson = """[{""HostName"":""localhost"",""Password"":""123456""}]""";

            var exception = Assert.ThrowsException<ProblemDetailsException>(() => serializer.Deserialize<SecuredConnectionInfos>(criticalJson));

            var json = exception.ProblemDetails.ToJson();

            Assert.AreEqual(exception.ProblemDetails.Extensions["jsonString"], "Hidden cause of security critical infos");
        }

        [TestMethod]
        public void Non_Security_Critial_Class_Have_To_Be_Print_Out_Json_String()
        {
            var serializer = ServiceProvider.GetRequiredService<IJsonSerializer>();
            var criticalJson = """[{""HostName"":""localhost"",""Password"":""123456""}]""";

            var exception = Assert.ThrowsException<ProblemDetailsException>(() => serializer.Deserialize<UnSecuredConnectionInfos>(criticalJson));

            Assert.AreEqual(exception.ProblemDetails.Extensions["jsonString"], criticalJson);
        }

        [TestMethod]
        public void Serialize_Camel_Case_Test()
        {
            var connectionInfos = new SecuredConnectionInfos()
            {
                HostName = "Hello",
                Password = "123"
            };

            var serializer = ServiceProvider.GetRequiredService<IJsonSerializer>();
            var json = serializer.Serialize(connectionInfos);

            Assert.AreEqual("{\"hostName\":\"Hello\",\"password\":\"123\"}", json);
        }
    }

    public class SecuredConnectionInfos
    {
        public string HostName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }


    [ShowJsonOnError]
    public class UnSecuredConnectionInfos
    {
        public string HostName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
