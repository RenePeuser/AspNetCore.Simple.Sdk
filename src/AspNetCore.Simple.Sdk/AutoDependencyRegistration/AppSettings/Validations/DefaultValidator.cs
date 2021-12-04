namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class DefaultValidator : SettingsValidator<DefaultValidator>
    {
        public override void Validate(DefaultValidator setting)
        {
            // Default validation, if no validator is given - nothing will be validated !
        }
    }
}