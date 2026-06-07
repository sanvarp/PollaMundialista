using PollaMundialista.Application.Auth;
using PollaMundialista.Application.Common;

namespace PollaMundialista.Application.Abstractions;

/// <summary>
/// Registration and login. Implemented in Infrastructure on top of ASP.NET Core Identity.
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}