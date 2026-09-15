using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Auth;

namespace LibraryManagement.AppServices.Interfaces;

/// <summary>Account registration and sign-in.</summary>
public interface IAuthService
{
    /// <summary>
    /// Creates a Member account and signs it straight in. Conflicts if the email is taken.
    /// </summary>
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies credentials and issues a token. An unknown email and a wrong password both
    /// come back Unauthorized with the same message.
    /// </summary>
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
