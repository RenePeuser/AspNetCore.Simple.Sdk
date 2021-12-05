# Startup
So for quick start you only have to derive from the oprimized startup class and you are ready to go


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
[ApiVersion("1.0")]
[ApiController]
[Route("v{version:apiVersion}/weather")]
public class WeatherForecastController : ControllerBase
{
	private static readonly string[] Summaries = new[]
	{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

	[HttpGet]
	public IEnumerable<WeatherForecast> Get()
	{
		var rng = new Random();



		return Enumerable.Range(1, 5).Select(index => new WeatherForecast
		{
			Date = new DateTime(2021, 11, index),
			TemperatureC = 32,
			Summary = Summaries[index]
		})
		.ToArray();
	}
}
```

## Step 4: Run and enjoy
![](first-api-result.png)