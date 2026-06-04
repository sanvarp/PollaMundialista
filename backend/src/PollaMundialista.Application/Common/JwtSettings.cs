namespace PollaMundialista.Application.Common;

/// <summary>
/// Strongly-typed JWT configuration bound from the "Jwt" config section.
/// Secret/Issuer/Audience must come from configuration / environment (never the repo).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
