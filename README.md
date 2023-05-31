# AspNetCore.Simple.Sdk

Target of this package is to create fast and clean Web-Api. 

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
With the provided `SimpleStartup ` base class anything is ready to use. Api-Versioning, Swagger and many more cool stuff.
```csharp
public sealed class Startup : SimpleStartup
{
	public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) :
		base(configuration, webHostEnvironment, new PathString("/api/my-api"))
	{
	}
}
```

### How to

<video src="https://renepeuser.visualstudio.com/_git/AspNetCore.Simple.Sdk?path=/docu/aspnetcore-simple-how-to.mp4" width=800 controls>
</video>
