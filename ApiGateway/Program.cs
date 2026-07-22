using Ocelot.DependencyInjection;
using Ocelot.Middleware;

#region Configuring Builder
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional:false, reloadOnChange:true); // Load Ocelot configuration from ocelot.json
builder.Services.AddOcelot(); // Add Ocelot services to the DI container
#endregion

var app = builder.Build();

#region Configuring App
await app.UseOcelot(); // Use Ocelot middleware to handle requests
#endregion

app.Run();
