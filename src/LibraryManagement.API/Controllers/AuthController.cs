using LibraryManagement.AppServices.DTOs.Auth;
using LibraryManagement.AppServices.DTOs.Common;
using LibraryManagement.AppServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

/// <summary>
/// Sign-up and sign-in. Both endpoints return the access token used by every other route.
/// </summary>
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Creates an account and returns a token for it. Self-registration always produces
    /// a Member; Admin accounts are seeded or promoted in the database.
    /// </summary>
    /// <param name="request">Name, email, password and password confirmation.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The new account and its access token.</response>
    /// <response code="400">A field failed validation.</response>
    /// <response code="409">An account with that email already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
        => HandleResult(await _authService.RegisterAsync(request, cancellationToken));

    /// <summary>
    /// Exchanges credentials for an access token. Paste the returned
    /// <c>accessToken</c> into the Authorize dialog to call the other endpoints.
    /// </summary>
    /// <param name="request">Email and password.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The account and its access token.</response>
    /// <response code="400">A field failed validation.</response>
    /// <response code="401">The email or password is wrong.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
        => HandleResult(await _authService.LoginAsync(request, cancellationToken));
}
