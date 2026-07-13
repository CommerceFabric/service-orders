using BusinessLogicLayer;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Policies;
using CommerceFabric.OrdersMicroservice.API.Middleware;
using DataAccessLayer;
using FluentValidation.AspNetCore;
using Polly;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Adding DAL and BLL services for dependency injection
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

// Add controllers to the service collection
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add Cors so that the frontend can access the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:4200") // our locally run angular app - needs to be changed to the actual domain when deployed
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add polly policies
builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

// Add auth
builder.Services.AddAuthorization();

// Add the HttpClient for the UsersMicroserviceClient, so that it can be injected into the OrdersService
var usersMicroserviceBaseUrl = builder.Configuration.GetValue<string>("UsersMicroservice:BaseUrl")!;
usersMicroserviceBaseUrl = usersMicroserviceBaseUrl.Replace("$USERS_MICROSERVICE_PORT", Environment.GetEnvironmentVariable("USERS_MICROSERVICE_PORT") ?? "9090"); // default port for UsersMicroservice
usersMicroserviceBaseUrl = usersMicroserviceBaseUrl.Replace("$USERS_MICROSERVICE_HOST", Environment.GetEnvironmentVariable("USERS_MICROSERVICE_HOST") ?? "localhost"); // default host for UsersMicroservice
usersMicroserviceBaseUrl = usersMicroserviceBaseUrl.Replace("$USERS_MICROSERVICE_SCHEME", Environment.GetEnvironmentVariable("USERS_MICROSERVICE_SCHEME") ?? "http"); // default scheme for UsersMicroservice

builder.Services.AddHttpClient<UsersMicroserviceClient>(
    client =>
    {
        client.BaseAddress = new Uri(usersMicroserviceBaseUrl);
    }
).AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy()
).AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCircuitBreakerPolicy()
);


// Add the HttpClient for the ProductsMicroserviceClient, so that it can be injected into the OrdersService
var productsMicroserviceBaseUrl = builder.Configuration.GetValue<string>("ProductsMicroservice:BaseUrl")!;
productsMicroserviceBaseUrl = productsMicroserviceBaseUrl.Replace("$PRODUCTS_MICROSERVICE_PORT", Environment.GetEnvironmentVariable("PRODUCTS_MICROSERVICE_PORT") ?? "5021"); // default port for ProductsMicroservice
productsMicroserviceBaseUrl = productsMicroserviceBaseUrl.Replace("$PRODUCTS_MICROSERVICE_HOST", Environment.GetEnvironmentVariable("PRODUCTS_MICROSERVICE_HOST") ?? "localhost"); // default host for ProductsMicroservice
productsMicroserviceBaseUrl = productsMicroserviceBaseUrl.Replace("$PRODUCTS_MICROSERVICE_SCHEME", Environment.GetEnvironmentVariable("PRODUCTS_MICROSERVICE_SCHEME") ?? "http"); // default scheme for ProductsMicroservice

builder.Services.AddHttpClient<ProductsMicroserviceClient>(
    client =>
    {
        client.BaseAddress = new Uri(productsMicroserviceBaseUrl);
    }
).AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetFallbackPolicy()
);

// build AFTER all registrations are done, so that the DI container is built with all the services
var app = builder.Build();

//Exception handling middleware
app.UseExceptionHandlingMiddleware();

// Enabling routing and Cors
app.UseRouting();
app.UseCors("AllowAll");

// use swagger
app.UseSwagger();
app.UseSwaggerUI();

// auth
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//endpoints
app.MapControllers();

app.Run();
