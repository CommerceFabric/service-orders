using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

#region Configuring Builder
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true); // Load Ocelot configuration from ocelot.json
builder.Services.AddOcelot().AddPolly(); // Add Ocelot services to the DI container + Polly for resilience and transient fault handling

// add CORS policy to allow requests from the Angular frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
#endregion

var app = builder.Build();

#region Configuring App
app.UseCors();
await app.UseOcelot(); // Use Ocelot middleware to handle requests
#endregion

app.Run();
