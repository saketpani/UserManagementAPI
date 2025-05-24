using System.Text;
using UserManagementAPI.Endpoints;
using UserManagementAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Add global error handling middleware first
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add middleware for request/response logging
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Add middleware for endpoint call counting
app.UseMiddleware<EndpointCallCounterMiddleware>();

// Map endpoints
app.MapUserEndpoints();

await app.RunAsync();

