using AspNetCore.Simple.Sdk.Authentication.Auth0;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Automapper
{
    // ToDo: Kind of automapper registration is not nice
    internal static class AddAutoMapperExtension
    {
        internal static void AddAutoMapper(this IServiceCollection services)
        {
            // Auto Mapper Configurations
            var mapperConfig = new MapperConfiguration(configure =>
            {
                configure.AddAuth0ResponseMapping();
                configure.AddAuth0RequestMapping();
            });

            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}
