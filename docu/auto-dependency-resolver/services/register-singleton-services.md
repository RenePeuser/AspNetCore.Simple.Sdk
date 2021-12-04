# Singleton service registration

## Singleton service wihout interface
A sample how simple you can register a service explicitly as Singleton service. If you dont set any information then the default registration is active.
```csharp
[ServiceRegistration(ServiceLifetime.Singleton)]
public class SingletonService
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<SingletonService>();
```

## Singleton service with interface
A sample how simple you can register a service explicitly as singleton service. If you dont set any information then the default registration is active.
```csharp
[ServiceRegistration(ServiceLifetime.Singleton)]
public class SingletonService : ISingletonService
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<SingletonService>();
```


## Singleton service with specific interface registration
For the case that your service derive from multiple interfaces and you want to specify just explicit one interface, then you can set it as parameter in the decorated attribute `ServiceRegistration` in this case `typeof(ISingletonServiceSpecific)`. This effects that this service `SingletonServiceWithSpecificInterface``will only be registered to interface `ISingletonServiceSpecific` 
```csharp
[ServiceRegistration(ServiceLifetime.Singleton, typeof(ISingletonServiceSpecific))]
public class SingletonServiceWithSpecificInterface : ISingletonService, ISingletonServiceSpecific
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<SingletonService>();
```