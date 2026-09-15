using LibraryManagement.Infrastructure.Entities;

namespace LibraryManagement.AppServices.Interfaces;

/// <summary>Issues the JSON Web Tokens used to authenticate API calls.</summary>
public interface ITokenService
{
    /// <summary>
    /// Signs a token carrying the user's id, name, email and role, and reports when it expires.
    /// </summary>
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
