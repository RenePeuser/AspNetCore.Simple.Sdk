namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public abstract class SettingsValidator<T> : ISettingsValidatorBase
    {
        public abstract void Validate(T setting);

        public void ValidateBase(object setting)
        {
            if (setting is T typedSetting)
            {
                Validate(typedSetting);
            }
        }
    }
}