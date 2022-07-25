# Transient setting registration

## Simple Settings sample from appsettings.json
A sample how simple you can register a setting explicitly as transient setting. If you dont set any information then the default registration is active.

```json
{
  "Settings": {
    "Name": "SonGoku"
  }
}
```
Imporant here is that you give the name of the settings are into the `AppSettingsRegistration` this is the key to find correct part of the configuration.
```csharp
[AppSettingsRegistration("Settings", typeof(TransientSettings), ServiceLifetime.Transient)]
public class TransientSettings
{
    public string Name { get; init; }
}
```

A sample for a simple setting without any kind of specifications
```csharp
// only register the class name for the settings. Anything else will be automatically done
autoRegistration.DoAutoRegistrationFor<Transientsetting>();
```