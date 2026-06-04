namespace PollaMundialista.Application.Common;

/// <summary>Canonical role names. Seeded in Infrastructure, used for [Authorize] in the API.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static readonly string[] All = [Admin, User];
}
