# AspNetCore.Simple.Sdk

Target of this package is to create fast and clean Web-Api.
You only need to configure in your appsettings needed and from
the package supported services and you can start implementing
your features which brings you your expected benefits

## Getting started

### Prerequisites
* [.Net 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
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
```csharp
public class Startup : SimpleStartup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) : base(configuration, webHostEnvironment, new PathString("/api/test"), "API for test")
    {
    }

    public override void AutoConfigureServices(AutoRegistration autoRegistration)
    {
        base.AutoConfigureServices(autoRegistration);

        // Hint: Try to use domain root extension to bundle the entry point for a specific domain.
        //       Makes your code maintainable, and nice to read !!
        autoRegistration.AddWeatherForecast();
    }
}
```
* Pathstring: this is the base path for all api routes, to reduce noising pasth in your controller implementations
* Swagger-Title: In this sample it is `API for test`

## Step 2: Documentation file
Go sure that your xml file for documentation will be generate, to get a good swagger documentation 

![](documentation-file.png)

## Step 3: First controller
Implement your first controller: (Here microsoft sample weahter app)

```csharp
public class SummariesProvider
{
    public IImmutableList<string> GetAll() => new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" }.ToImmutableList();
}

[ApiVersion("1.0")]
[ApiController]
[Route("v{version:apiVersion}/weather")]
public class WeatherForecastController : ControllerBase
{
    private readonly SummariesProvider _summariesProvider;

    public WeatherForecastController(SummariesProvider summariesProvider)
    {
        _summariesProvider = summariesProvider;
    }


    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = new DateTime(2021, 11, index),
            TemperatureC = 32,
            Summary = _summariesProvider.GetAll()[index]
        }).ToArray();
    }
}
```

## Step 4: Run and enjoy
![](/docu/first-api-result.png)

