using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Endpoints;

public static class AuthenticationEndpoint
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", ([FromBody] AuthenticationRequest request, IAuthenticationService authService) =>
        {
            try
            {
                var response = authService.Authenticate(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .AllowAnonymous();
    }
}
