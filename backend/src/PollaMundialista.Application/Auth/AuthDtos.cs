namespace PollaMundialista.Application.Auth;

public record RegisterRequest(string Email, string Password, string DisplayName);

public record LoginRequest(string Email, string Password);

/// <summary>Payload returned on successful register/login (see API contract §6).</summary>
public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Role, string DisplayName);