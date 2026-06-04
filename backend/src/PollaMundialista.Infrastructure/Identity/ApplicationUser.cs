using Microsoft.AspNetCore.Identity;

namespace PollaMundialista.Infrastructure.Identity;

/// <summary>
/// Identity user with a public display name. Kept in Infrastructure so the Domain
/// stays free of any Identity dependency (domain entities reference users by string id).
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
