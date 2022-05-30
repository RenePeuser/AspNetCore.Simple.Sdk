using System;
using System.Reflection;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.AutoDependencyRegistration;
using AspNetCore.Simple.Sdk.Cors;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Logger.Errors;
using AspNetCore.Simple.Sdk.Security;
using AspNetCore.Simple.Sdk.Serializer.Json;
using AspNetCore.Simple.Sdk.Swagger;
using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Startups
{
    public abstract class SimpleStartup
    {
        private Lazy<AutoRegistration> _lazyAutoRegistration = new();

        protected SimpleStartup(IConfiguration configuration,
                                IWebHostEnvironment webHostEnvironment,
                                PathString basePath,
                                string swaggerApiTitle) : this(configuration, webHostEnvironment, Assembly.GetCallingAssembly(), basePath, swaggerApiTitle)
        {
        }

        protected SimpleStartup(IConfiguration configuration,
                                IWebHostEnvironment webHostEnvironment,
                                Assembly assembly,
                                PathString basePath,
                                string swaggerApiTitle)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
            Assembly = assembly;
            BasePath = basePath;
            SwaggerApiTitle = swaggerApiTitle;
        }

        protected IWebHostEnvironment WebHostEnvironment { get; }

        protected Assembly Assembly { get; }

        protected PathString BasePath { get; }

        public string SwaggerApiTitle { get; }

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
            services.AddSwaggerGenSimplified(Assembly, SwaggerApiTitle);

            services.AddControllers();
            services.AddQuerySecurityFilter();
            services.AddJsonContentNegotiation();

            services.AddSingleton(typeof(Assembly), Assembly);
            services.AddHttpClient();
            services.AddErrorHandling();
            services.AddErrorLogging();

            services.AddApiVersioningSimplified();

            services.AddJsonSerializer();

            services.AddCorsSettings(Configuration);

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
            app.UseSwaggerSimplified(BasePath);
            app.UseSwaggerUiSimplified(Assembly, BasePath);

            app.UsePathBase(BasePath);

            app.UseErrorHandling();

            app.UseErrorLogging();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCorsConfiguration();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseOptions();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
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
