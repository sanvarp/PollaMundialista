using Microsoft.AspNetCore.Identity;

using PollaMundialista.Application.Abstractions;
using PollaMundialista.Application.Auth;
using PollaMundialista.Application.Common;

namespace PollaMundialista.Infrastructure.Identity;

/// <summary>
/// Register/login on top of ASP.NET Core Identity. New users get the <c>User</c> role.
/// Passwords are hashed by Identity's password hasher (never stored in clear).
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Result<AuthResponse>.Fail("Ya existe una cuenta con ese correo.", StatusCodes.Status409Conflict);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName.Trim(),
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            var error = string.Join(" ", created.Errors.Select(e => e.Description));
            return Result<AuthResponse>.Fail(error, StatusCodes.Status400BadRequest);
        }

        await _userManager.AddToRoleAsync(user, Roles.User);
        return Result<AuthResponse>.Ok(await BuildResponseAsync(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Result<AuthResponse>.Fail("Correo o contraseña incorrectos.", StatusCodes.Status401Unauthorized);

        return Result<AuthResponse>.Ok(await BuildResponseAsync(user));
    }

    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.Contains(Roles.Admin) ? Roles.Admin : Roles.User;
        var token = _tokenService.CreateToken(user.Id, user.Email!, user.DisplayName, roles);
        return new AuthResponse(token.Token, token.ExpiresAtUtc, primaryRole, user.DisplayName);
    }
}

// Small constant to avoid magic strings; StatusCodes lives in Microsoft.AspNetCore.Http.
internal static class StatusCodes
{
    public const int Status400BadRequest = 400;
    public const int Status401Unauthorized = 401;
    public const int Status409Conflict = 409;
}