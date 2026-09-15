namespace LibraryManagement.AppServices.DTOs.Auth;

/// <summary>Returned by register and login.</summary>
public class AuthResponse
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string TokenType { get; set; } = "Bearer";
}
