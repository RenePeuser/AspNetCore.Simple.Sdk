# Settings validation
It can makes sense to you implement a validation for some specific settings. Important for good code is that you never inject classes or settings which are not valid ! If the consumer of a class have to check the consistency of an injected dependency then you have a bad design!

## Simple Settings sample from appsettings.json
A sample how simple you can register a setting explicitly as singleton setting. If you dont set any information then the default registration is active.

```json
{
  "Settings": {
    "Name": "SonGoku"
  }
}
```
Imporant here is that you give the name of the settings are into the `AppSettingsRegistration` this is the key to find correct part of the configuration.
```csharp
[AppSettingsRegistration("Settings", typeof(ScopedSettingsWithValidator), ServiceLifetime.Scoped, typeof(ScopeSettingsValidator))]
public class ScopedSettingsWithValidator
{
    public string Name { get; init; }
}
```
You can implement any kind of logic here. The only restriction you have to derive from `SettingsValidator<T>` base class
```csharp
public class ScopeSettingsValidator : SettingsValidator<ScopedSettingsWithValidator>
{
    public override void Validate(ScopedSettingsWithValidator setting)
    {
        var expectedValue = "Son Goku";
        if (setting.Name.NotEqualsTo(expectedValue))
        {
            throw new ArgumentException($"The property: '{nameof(ScopedSettingsWithValidator.Name)}' does not match expected value: '{expectedValue}'");
        }
    }
}
```

A sample for a simple setting without any kind of specifications
```csharp
// if you try to register this settings manualy or automatically as dependencies it will call first the validator and if your validation throws exception the program stoped.
autoRegistration.DoAutoRegistrationFor<ScopedSettingsWithValidator>();
```