namespace UserManagementAPI.Models;

public class AuthenticationRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class AuthenticationResponse
{
    public string Token { get; set; } = default!;
    public string Username { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
}
