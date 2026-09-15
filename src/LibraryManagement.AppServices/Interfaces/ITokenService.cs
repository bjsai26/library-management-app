using LibraryManagement.Infrastructure.Entities;

namespace LibraryManagement.AppServices.Interfaces;

/// <summary>Issues the JSON Web Tokens used to authenticate API calls.</summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
