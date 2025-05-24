using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public interface IAuthenticationService
{
    AuthenticationResponse Authenticate(AuthenticationRequest request);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly IDictionary<string, string> _users = new Dictionary<string, string>
    {
        { "admin", "admin123" }, // In production, use hashed passwords and proper user storage
    };

    public AuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthenticationResponse Authenticate(AuthenticationRequest request)
    {
        // Validate username and password
        if (!_users.TryGetValue(request.Username, out var password) ||
            password != request.Password)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Generate JWT token
        var token = GenerateJwtToken(request.Username);

        return new AuthenticationResponse
        {
            Token = token,
            Username = request.Username,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }
    private string GenerateJwtToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ??
            throw new InvalidOperationException("JWT Secret Key is not configured"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
