using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    // Hint: only used to go sure that different registration attributes can not mixed with multiple once.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class RegistrationBaseAttribute : Attribute
    {

    }
}
