using System;
using System.Reflection;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddMediator(Assembly);
            services.AddValidationBehavior();

            services.AddControllers();
            services.AddQuerySecurityFilter(Configuration);
            services.AddJsonContentNegotiation();

            services.AddSingleton(typeof(Assembly), Assembly);
            services.AddHttpClient();
            services.AddErrorHandling();
            services.AddErrorLogging();
            services.AddApiVersionProvider();
            services.AddApiVersioningSimplified();
            services.AddJsonSerializer();
            services.AddCorsSettings(Configuration);

            services.AddBackOff();

            services.AddOAuthAuthentication(Configuration);

            // Activate mediator for current assembly and calling once
            services.AddMediator();
            services.AddMediator(Assembly);
            services.AddMediatRCaching(Assembly);

            services.AddRedisCache(Configuration, Logger);

            services.AddSwaggerGenSimplified(Assembly, Configuration, Logger);

            services.AddAzureBlobStorage(Configuration);
            services.AddAzureBlobStorageFactory();

            services.AddApplicationInsights(Configuration);

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

            // app.UseOptions();

            app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireAuthorization(); });
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
}
