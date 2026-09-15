using LibraryManagement.Infrastructure.Enums;

namespace LibraryManagement.Infrastructure.Entities;

/// <summary>An account that can sign in to the API.</summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string PasswordSalt { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Member;
}
