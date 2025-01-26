using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class MissingRegistrationStrategyException(string message) : Exception(message);
}
