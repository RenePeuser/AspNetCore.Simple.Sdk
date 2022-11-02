using System;
using AspNetCore.Simple.Sdk.Authentication.Auth0;
using AspNetCore.Simple.Sdk.Extensions;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Automapper
{
    // ToDo: Kind of automapper registration is not nice
    internal static class AddAutoMapperExtension
    {
        public static void AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(_ => { });
        }

        public static void AddAutoMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configure)
        {
            //if (services.IsAlreadyRegistered<IMapper>())
            //{
            //    return;
            //}

            // Auto Mapper Configurations
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddAuth0ResponseMapping();
                config.AddAuth0RequestMapping();

                // Configure stuff from consumer outside of this lib.
                configure(config);
            });

            var mapper = mapperConfig.CreateMapper();
            services.AddSingletonIfNotExists(mapper);
        }
    }
}
