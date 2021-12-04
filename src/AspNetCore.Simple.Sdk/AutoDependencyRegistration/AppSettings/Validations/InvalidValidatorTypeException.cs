using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class InvalidValidatorTypeException : Exception
    {
        public InvalidValidatorTypeException(Type type) : base($"The validator type: {type.Name} is not implementing: '{nameof(SettingsValidator<object>)}'. Please check your implementation.")
        {

        }
    }
}
