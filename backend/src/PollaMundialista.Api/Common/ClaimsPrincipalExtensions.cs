using System.Security.Claims;

namespace PollaMundialista.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The authenticated user's id (JWT 'sub', mapped to NameIdentifier).</summary>
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Authenticated request is missing the user id claim.");
}