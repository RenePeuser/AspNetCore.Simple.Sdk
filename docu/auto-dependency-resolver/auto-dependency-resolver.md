## Use with SimpleStartup
```csharp
public class Startup : SimpleStartup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) : base(configuration, webHostEnvironment, new PathString("/api/test"), "API for test")
    {
    }

    // Called internally from ConfigureServices
    public override void AutoConfigureServices(AutoRegistration autoRegistration)
    {
        base.AutoConfigureServices(autoRegistration);

        autoRegistration.DoAutoRegistrationFor<SummariesProvider>();
    }

        // Called internally from ConfigureDevelopmentServices
    public override void AutoConfigureDevelopmentServices(AutoRegistration autoRegistration)
    {
        base.AutoConfigureServices(autoRegistration);

        autoRegistration.DoAutoRegistrationFor<SummariesProvider>();
    }
}
```

## Mnualy usage via factory
```csharp
public class Startup
{
    private IConfiguration _configuration;
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var autoRegistration = new AutoRegistrationFactory().Create(services, _configuration);
        
        autoRegistration.DoAutoRegistrationFor<SummariesProvider>();
    }
}
```

## Services
* [Scoped](../auto-dependency-resolver/services/register-transient-services.md)
* [Transient](../auto-dependency-resolver/services/register-transient-services.md)
* [Singleton](../auto-dependency-resolver/services/register-transient-services.md)

## Appsettings
* [Scoped](../auto-dependency-resolver/settings/register-scoped-settings.md)
* [Transient](../auto-dependency-resolver/settings/register-transient-settings.md)
* [Singleton](../auto-dependency-resolver/settings/register-singleton-settings.md)
* [Validation](../auto-dependency-resolver/settings/register-singleton-settings.md)
