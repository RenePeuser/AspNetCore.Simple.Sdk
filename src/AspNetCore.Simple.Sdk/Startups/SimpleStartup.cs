using System.Reflection;
using AspNetCore.Simple.Sdk.ApiVersioning;
using AspNetCore.Simple.Sdk.ErrorHandling.Development;
using AspNetCore.Simple.Sdk.ErrorHandling.Production;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Logger.Errors.Middlewares;
using AspNetCore.Simple.Sdk.Security;
using AspNetCore.Simple.Sdk.Serializer.Json;
using AspNetCore.Simple.Sdk.Swagger;
using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.Simple.Sdk.Startups
{
    public abstract class SimpleStartup
    {
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
            services.AddErrorHandlingDevelopment();

            ConfigureServices(services);
        }

        // This method gets called by the runtime if there is no expicit "Production configure method.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGenSimplified(Assembly, SwaggerApiTitle);

            services.AddControllers();
            services.AddQuerySecurityFilter();
            services.AddJsonContentNegotiation();

            services.AddSingleton(typeof(Assembly), Assembly);
            services.AddHttpClient();
            services.AddErrorHandlingProduction();
            services.AddErrorLogging();

            services.AddApiVersioningSimplified();

            services.AddJsonSerializer();
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

            app.UseCors("AllowAll");

            app.UsePathBase(BasePath);

            if (WebHostEnvironment.IsDevelopment().IsFalse())
            {
                app.UseErrorHandlingProduction();
            }

            app.UseErrorLogging();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
