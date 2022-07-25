# Transient service registration

## Transient service wihout interface
A sample how simple you can register a service explicitly as transient service. If you dont set any information then the default registration is active.
```csharp
[ServiceRegistration(ServiceLifetime.Transient)]
public class TransientService
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<TransientService>();
```

## Transient service with interface
A sample how simple you can register a service explicitly as transient service. If you dont set any information then the default registration is active.
```csharp
[ServiceRegistration(ServiceLifetime.Transient)]
public class TransientService : ITransientService
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<TransientService>();
```


## Transient service with specific interface registration
For the case that your service derive from multiple interfaces and you want to specify just explicit one interface, then you can set it as parameter in the decorated attribute `ServiceRegistration` in this case `typeof(ITransientServiceSpecific)`. This effects that this service `TransientServiceWithSpecificInterface``will only be registered to interface `ITransientServiceSpecific` 
```csharp
[ServiceRegistration(ServiceLifetime.Transient, typeof(ITransientServiceSpecific))]
public class TransientServiceWithSpecificInterface : ITransientService, ITransientServiceSpecific
{
}
```

A sample for a simple service without any kind of specifications
```csharp
// only register the class name (implementation). The interface - if it implements one it will be registered automatically
autoRegistration.DoAutoRegistrationFor<TransientService>();
```