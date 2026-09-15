using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Auth;

namespace LibraryManagement.AppServices.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
