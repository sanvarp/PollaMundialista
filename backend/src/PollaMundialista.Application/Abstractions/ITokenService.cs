namespace PollaMundialista.Application.Abstractions;

/// <summary>Issued JWT plus its absolute UTC expiry.</summary>
public record TokenResult(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// Creates signed JWTs. Defined in Application (provider-agnostic, takes primitives
/// so it never depends on the Identity user type); implemented in Infrastructure.
/// </summary>
public interface ITokenService
{
    TokenResult CreateToken(string userId, string email, string displayName, IEnumerable<string> roles);
}
