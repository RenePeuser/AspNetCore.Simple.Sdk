# AspNetCore.Simple.Sdk

Target of this package is to create fast and clean Web-Api.
You only need to configure in your appsettings needed and from
the package supported services and you can start implementing
your features which brings you your expected benefits

## Getting started

### Prerequisites
* [.Net 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
* [Asp.Net Web-Api](https://learn.microsoft.com/de-de/aspnet/web-api/overview/getting-started-with-aspnet-web-api/tutorial-your-first-web-api)

### Install the package

```dotnetcli
dotnet add package AspNetCore.Simple.Sdk
```

## Samples

### Basic concept
# Startup
So for quick start you only have to derive from the optimized startup class and you are ready to go

```
Hint: The hold sample is inside this repo !
```

## Step 1: Startup
* Pathstring: this is the base path for all api routes, to reduce noising pasth in your controller implementations

```csharp
public class Startup : SimpleStartup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) : base(configuration, webHostEnvironment, new PathString("/api/test"))
    {
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        
        // Your domain registrations
    }
}
```

### Content
* [Authentication](/docu/authentication.md)
* [Security filters](/docu/security-filters.md)
* [Cors](/docu/cors.md)
* [Swagger](/docu/swagger.md)
* [ErrorHandling](/docu/error-handling.md)


