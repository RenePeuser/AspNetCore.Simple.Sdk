using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.ApplicationInsight;
using AspNetCore.Simple.Sdk.Authentication.Auth0;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using AspNetCore.Simple.Sdk.Caching;
using AspNetCore.Simple.Sdk.Cors;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Logger.Errors;
using AspNetCore.Simple.Sdk.Mediator;
using AspNetCore.Simple.Sdk.Polly;
using AspNetCore.Simple.Sdk.Security;
using AspNetCore.Simple.Sdk.Serializer.Json;
using AspNetCore.Simple.Sdk.Storage;
using AspNetCore.Simple.Sdk.Swagger;
using Extensions.Pack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.OpenApi; // Removed - not needed for .NET 10 with Swashbuckle
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// using Microsoft.OpenApi.Models; // Not needed - commented out transformers

namespace AspNetCore.Simple.Sdk.Startups
{
    public static class AddBasePathExtension
    {
        public static void AddBasePath(this IServiceCollection services, PathString pathString)
        {
            services.AddSingletonIfNotExists(new BasePath(pathString));
        }
    }

    public record BasePath(PathString Value);

    public abstract class SimpleStartup
    {
        private Lazy<AutoRegistration> _lazyAutoRegistration = new();
        private readonly ApiVersionProvider _apiVersionProvider = new ApiVersionProvider(new AssemblyTypeProvider());

        protected SimpleStartup(IConfiguration configuration,
                                IWebHostEnvironment webHostEnvironment,
                                PathString basePath) : this(configuration, webHostEnvironment, Assembly.GetCallingAssembly(), basePath)
        {
        }

        protected SimpleStartup(IConfiguration configuration,
                                IWebHostEnvironment webHostEnvironment,
                                Assembly assembly,
                                PathString basePath)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
            Assembly = assembly;
            BasePath = basePath;
            Logger = new Logger<SimpleStartup>(LoggerFactory.Create(logBuilder => logBuilder.AddConsole().AddDebug()));

            // Important to handle multiple test runs which start stops quickly.
            AddMediatorExtension.RegisteredMediators.Clear();
        }

        protected ILogger Logger { get; }

        protected IWebHostEnvironment WebHostEnvironment { get; }

        protected Assembly Assembly { get; }

        protected PathString BasePath { get; }

        protected IConfiguration Configuration { get; }

        public virtual void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureServices(services);
            AutoConfigureDevelopmentServices(GetAutoRegistration(services, Configuration));
        }

        public virtual void AutoConfigureDevelopmentServices(AutoRegistration autoRegistration)
        {
            // only optional for user
        }

        // This method gets called by the runtime if there is no explicit "Production configure method.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddBasePath(BasePath);
            services.AddValidationBehavior();

            services.AddControllers();
            services.AddQuerySecurityFilter(Configuration);
            services.AddJsonContentNegotiation();

            services.AddSingleton(Assembly);
            services.AddHttpClient();
            services.AddErrorHandling();
            services.AddErrorLogging();
            services.AddApiVersionProvider();
            services.AddApiVersioningSimplified();
            services.AddJsonSerializer();
            services.AddCorsSettings(Configuration);
            services.AddBackOff();

            services.AddOAuthAuthentication(Configuration);

            services.AddApplicationInsights(Configuration);

            services.AddRedisCache(Configuration, Logger);

            services.AddSwaggerGenSimplified(Assembly, Configuration, Logger);

            services.AddAzureBlobStorage(Configuration);
            services.AddAzureBlobStorageFactory(Configuration);

            // Register all versions of existing APIs
            // Note: AddOpenApi and Document Transformers are part of Microsoft.AspNetCore.OpenApi
            // which conflicts with Swashbuckle in .NET 10. Using Swashbuckle only.
            // var versions = _apiVersionProvider.GetAllApiVersions(Assembly);
            // foreach (var version in versions)
            // {
            //     services.AddOpenApi($"v{version.MajorVersion}", options =>
            //                               {
            //                                   options.AddDocumentTransformer<DocumentInfosTransformer>();
            //                                   options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            //                               });
            // }

            // Activate mediator for current assembly and calling once
            services.AddMediator();
            services.AddMediator(Assembly);
            services.AddMediatRCaching(Assembly);

            AutoConfigureServices(GetAutoRegistration(services, Configuration));
        }

        public virtual void AutoConfigureServices(AutoRegistration autoRegistration)
        {
            // only optional for user
        }

        public virtual void ConfigureDevelopment(IApplicationBuilder app)
        {
            Configure(app);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseSwaggerSimplified(Configuration, BasePath, Logger);
            app.UseSwaggerUiSimplified(Assembly, BasePath);

            app.UseErrorHandling();

            app.UseErrorLogging();
            app.UseHttpsRedirection();

            app.UsePathBase(BasePath);

            app.UseRouting();

            app.UseCorsConfiguration();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
                             {
                                 // endpoints.MapOpenApi(); // Part of Microsoft.AspNetCore.OpenApi - using Swashbuckle instead
                                 endpoints.MapControllers().RequireAuthorization();
                             });
        }

        private AutoRegistration GetAutoRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            if (_lazyAutoRegistration.IsValueCreated.IsFalse())
            {
                _lazyAutoRegistration = new Lazy<AutoRegistration>(() => new AutoRegistrationFactory().Create(serviceCollection, configuration));
            }

            return _lazyAutoRegistration.Value;
        }
    }


    // Commented out - these transformers use Microsoft.AspNetCore.OpenApi which we removed
    // Swashbuckle's own filters and configuration handle this functionality
    /*
    public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider,
                                                          SwaggerInfos swaggerInfos) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false);
            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                var requirements = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer", // "bearer" refers to the header name here
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token"
                    }
                };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = requirements;
            }

            var documentSpecificInfos = swaggerInfos.SwaggerInfosByVersion.FirstOrDefault(doc => doc.Version.Contains(context.DocumentName.TrimStart('v').TrimStart('V')));

            if (documentSpecificInfos.IsNotNull())
            {
                document.Info = new()
                {
                    Title = documentSpecificInfos.Title,
                    Version = documentSpecificInfos.Version,
                    Description = documentSpecificInfos.Description,
                    Contact = new OpenApiContact()
                    {
                        Email = documentSpecificInfos.ContactEmail,
                        Name = documentSpecificInfos.ContactName,
                    }
                };
            }
            else
            {
                document.Info = new()
                {
                    Title = "Your first Open API",
                    Version = "v1",
                    Description = "API for Damien"
                };
            }
        }
    }

    public sealed class DocumentInfosTransformer(SwaggerInfos swaggerInfos) : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var documentSpecificInfos = swaggerInfos.SwaggerInfosByVersion.FirstOrDefault(doc => doc.Version.Contains(context.DocumentName.TrimStart('v').TrimStart('V')));

            if (documentSpecificInfos.IsNotNull())
            {
                document.Info = new()
                {
                    Title = documentSpecificInfos.Title,
                    Version = documentSpecificInfos.Version,
                    Description = documentSpecificInfos.Description,
                    Contact = new OpenApiContact()
                    {
                        Email = documentSpecificInfos.ContactEmail,
                        Name = documentSpecificInfos.ContactName,
                    }
                };
            }
            else
            {
                document.Info = new()
                {
                    Title = "Your first Open API",
                    Version = "v1",
                    Description = "API for Damien"
                };
            }

            return Task.CompletedTask;
        }
    }
    */
}
