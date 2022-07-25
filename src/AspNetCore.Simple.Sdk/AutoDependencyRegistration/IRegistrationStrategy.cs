using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal interface IRegistrationStrategy
    {
        bool DoRegistrationFor(Type type, bool registrationDone);
    }
}
