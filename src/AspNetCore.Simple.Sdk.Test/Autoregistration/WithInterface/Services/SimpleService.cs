namespace AspNetCore.Simple.Sdk.Test.Autoregistration.WithInterface.Services
{
    public class SimpleService : ISimpleService
    {
        public bool Invoke()
        {
            return true;
        }
    }

    public interface ISimpleService
    {
    }
}
