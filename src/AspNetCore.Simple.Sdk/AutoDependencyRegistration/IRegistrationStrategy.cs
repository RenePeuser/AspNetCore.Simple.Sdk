using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal interface IRegistrationStrategy
    {
        bool DoAutoRegistration(Type type, bool registrationDone);
    }
}