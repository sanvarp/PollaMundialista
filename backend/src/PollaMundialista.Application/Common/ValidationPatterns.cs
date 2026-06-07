namespace PollaMundialista.Application.Common;

public static class ValidationPatterns
{
    /// <summary>
    /// Email must be local@domain.tld with a 2+ letter TLD (rejects "a@b").
    /// Mirrors the frontend EMAIL_PATTERN. FluentValidation's default EmailAddress()
    /// only checks for an '@', so we add this for a stricter, consistent rule.
    /// </summary>
    public const string Email = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
}