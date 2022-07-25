namespace AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy.Services
{
    public class SimpleService : ISimpleService
    {
        public bool Invoke() => true;
    }

    public interface ISimpleService
    {
    }
}
