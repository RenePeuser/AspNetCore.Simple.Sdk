# Scoped service registration

## Scoped service wihout interface
A sample how simple you can register a service explicitly as scoped service. If you dont set any information then the default registration is active.
```csharp
[ServiceRegistration(ServiceLifetime.Scoped)]
public class ScopedService
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<ScopedService>();
```

## Scoped service with interface
A sample how simple you can register a service explicitly as scoped service. If you dont set any information then the default registration is active.
```csharp
[ServiceRegistration(ServiceLifetime.Scoped)]
public class ScopedService : IScopedService
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<ScopedService>();
```


## Scoped service with specific interface registration
For the case that your service derive from multiple interfaces and you want to specify just explicit one interface, then you can set it as parameter in the decorated attribute `ServiceRegistration` in this case `typeof(IScopedServiceSpecific)`. This effects that this service `ScopedServiceWithSpecificInterface``will only be registered to interface `IScopedServiceSpecific` 
```csharp
[ServiceRegistration(ServiceLifetime.Scoped, typeof(IScopedServiceSpecific))]
public class ScopedServiceWithSpecificInterface : IScopedService, IScopedServiceSpecific
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<ScopedService>();
```