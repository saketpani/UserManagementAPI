using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using UserManagementAPI.Models;

namespace UserManagementAPI.Endpoints
{
    public static class UserEndpoint
    {
        private static readonly List<User> users = [];
        private static int nextId = 1;

        public static void MapUserEndpoints(this WebApplication app)
        {
            // Create user
            app.MapPost("/users", (User user) =>
            {
                user.Id = nextId++;
                users.Add(user);
                return Results.Created($"/users/{user.Id}", user);
            });

            // Get all users
            app.MapGet("/users", () => users);

            // Get user by id
            app.MapGet("/users/{id:int}", (int id) =>
            {
                var user = users.FirstOrDefault(u => u.Id == id);
                return user is not null ? Results.Ok(user) : Results.NotFound();
            });

            // Update user
            app.MapPut("/users/{id:int}", (int id, User updatedUser) =>
            {
                var user = users.FirstOrDefault(u => u.Id == id);
                if (user is null) return Results.NotFound();
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                return Results.NoContent();
            });

            // Delete user
            app.MapDelete("/users/{id:int}", (int id) =>
            {
                var user = users.FirstOrDefault(u => u.Id == id);
                if (user is null) return Results.NotFound();
                users.Remove(user);
                return Results.NoContent();
            });
        }
    }
}