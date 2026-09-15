using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Auth;
using LibraryManagement.AppServices.Interfaces;
using LibraryManagement.AppServices.Security;
using LibraryManagement.Infrastructure.Entities;
using LibraryManagement.Infrastructure.Enums;
using LibraryManagement.Infrastructure.Repositories;

namespace LibraryManagement.AppServices.Services;

/// <inheritdoc cref="IAuthService" />
public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IGenericRepository<User> users,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.ExistsAsync(u => u.Email == email, cancellationToken))
        {
            return ServiceResult<AuthResponse>.Conflict("An account with this email already exists.");
        }

        var (hash, salt) = _passwordHasher.Hash(request.Password);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            // Self-registration always produces a Member; admins are seeded or promoted in the database.
            Role = UserRole.Member
        };

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthResponse>.Success(BuildResponse(user), "Registration successful.");
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // One message covers both an unknown email and a wrong password, so the endpoint
        // does not reveal which accounts exist.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return ServiceResult<AuthResponse>.Unauthorized("Invalid email or password.");
        }

        return ServiceResult<AuthResponse>.Success(BuildResponse(user), "Login successful.");
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = _tokenService.CreateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = token,
            ExpiresAt = expiresAt
        };
    }
}
