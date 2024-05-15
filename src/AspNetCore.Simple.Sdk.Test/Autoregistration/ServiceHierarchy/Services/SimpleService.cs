namespace AspNetCore.Simple.Sdk.Test.Autoregistration.ServiceHierarchy.Services
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
