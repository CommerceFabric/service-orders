using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Inject the data access layer services into the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
        {
            #region Adding MongoDB services with connection string from appsettings.json and environment variables
            // replace connection string template from appsettings.json with actual values from environment variables
            var connectionString = configuration.GetConnectionString("MongoDB")!;
            connectionString = connectionString.Replace("$ORDERS_MONGODB_PORT", Environment.GetEnvironmentVariable("ORDERS_MONGODB_PORT") ?? "27017"); // default port for MongoDB
            connectionString = connectionString.Replace("$ORDERS_MONGODB_HOST", Environment.GetEnvironmentVariable("ORDERS_MONGODB_HOST") ?? "localhost");

            // now add mongo db with the connection string
            services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString)); // must be singleton, as MongoClient is thread-safe and should be reused across the application
            services.AddScoped<IMongoDatabase>(sp =>
                sp.GetRequiredService<IMongoClient>()
                    .GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE") ?? "OrdersDb")); // if OrdersDb does not exist, MongoDB will create it when the first document is inserted; else, it will use the existing database
            #endregion

            // Add custom services
            services.AddScoped<IOrdersRepository, OrdersRepository>();

            return services;
        }
    }
}
