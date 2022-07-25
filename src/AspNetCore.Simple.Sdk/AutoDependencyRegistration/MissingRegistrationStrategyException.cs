using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class MissingRegistrationStrategyException : Exception
    {
        public MissingRegistrationStrategyException(string message) : base(message)
        {
        }
    }
}