using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            // Get the appropriate status code and message based on the exception type
            (response.StatusCode, var message) = error switch
            {
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found"),
                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized access"),
                ArgumentException => ((int)HttpStatusCode.BadRequest, error.Message),
                _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            _logger.LogError(error, "Request failed with Status Code: {StatusCode}, Message: {Message}",
                response.StatusCode, error.Message);

            var result = JsonSerializer.Serialize(new
            {
                StatusCode = response.StatusCode,
                Message = message,
                // Only include detailed error information in development
                DetailedError = _env.IsDevelopment() ? new
                {
                    error.Message,
                    error.Source,
                    StackTrace = error.StackTrace?.Split('\n')
                } : null
            });

            await response.WriteAsync(result);
        }
    }
}
