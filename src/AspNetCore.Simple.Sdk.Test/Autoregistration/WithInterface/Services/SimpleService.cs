namespace AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface.Services
{
    public class SimpleService : ISimpleService
    {
        public bool Invoke() => true;
    }

    public interface ISimpleService
    {
    }
}
