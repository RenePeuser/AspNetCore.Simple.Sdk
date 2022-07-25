using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class AutoRegistration
    {
        private readonly TypeRegistration _typeRegistrationStrategy;

        internal AutoRegistration(TypeRegistration typeRegistrationStrategy)
        {
            _typeRegistrationStrategy = typeRegistrationStrategy;
        }

        public void DoAutoRegistrationFor<T>()
        {
            DoAutoRegistrationFor(typeof(T));
        }

        public void DoAutoRegistrationFor(Type type)
        {
            _typeRegistrationStrategy.DoAutoRegistration(type);
        }
    }
}
