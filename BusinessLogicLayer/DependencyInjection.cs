using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Inject the business logic layer services into the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Add AutoMapper to map between DTOs and Entities
            services.AddAutoMapper(
                cfg => { },
                typeof(OrderMappingProfile),
                typeof(OrderItemMappingProfile)
            );

            // Add Fluentvalidations to use as contract validators for the DTOs
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly); // don't need to do this per validator, as it will automatically scan the assembly for all validators and register them in the DI container

            // Add custom services
            services.AddScoped<IOrdersService, OrdersService>();

            // Add Redis cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
            });

            // Add RabbitMQ services
            services.AddSingleton<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>(); // singleton because we want to have only one instance of the consumer that will be used throughout the application
            services.AddHostedService<RabbitMQProductNameUpdateHostedService>(); // hosted service because we want to run the consumer in the background and listen for messages from RabbitMQ

            return services;
        }
    }
}
