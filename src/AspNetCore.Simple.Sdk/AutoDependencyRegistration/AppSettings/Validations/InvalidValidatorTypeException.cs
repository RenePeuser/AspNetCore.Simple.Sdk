using System;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class InvalidValidatorTypeException(Type type) : Exception($"The validator type: {type.Name} is not implementing: '{nameof(SettingsValidator<object>)}'. Please check your implementation.");

    public class InvalidCustomTypeRegistrationException(Type type) : Exception($"The custom registration type: {type.Name} is not implementing: '{nameof(ICustomTypeRegistration)}'. Please check your implementation.");
}
