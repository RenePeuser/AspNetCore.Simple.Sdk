using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class MissingRegistrationStrategyException : Exception
    {
        public MissingRegistrationStrategyException(string message) : base(message)
        {
        }
    }
}
