namespace PollaMundialista.Application.Common;

/// <summary>Minimal user projection exposed to the Application layer (no Identity dependency).</summary>
public record UserSummary(string Id, string DisplayName);