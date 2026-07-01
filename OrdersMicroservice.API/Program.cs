using BusinessLogicLayer;
using CommerceFabric.OrdersMicroservice.API.Middleware;
using DataAccessLayer;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Adding DAL and BLL services for dependency injection
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

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

//Exception handling middleware
app.UseExceptionHandlingMiddleware();

// Enabling routing and Cors
app.UseRouting();
app.UseCors();

// use swagger
app.UseSwagger();
app.UseSwaggerUI();

// auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//endpoints
app.MapControllers();

app.Run();
