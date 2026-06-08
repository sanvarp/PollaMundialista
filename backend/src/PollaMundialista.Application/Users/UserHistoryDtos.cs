using PollaMundialista.Application.Matches;

namespace PollaMundialista.Application.Users;

/// <summary>
/// One row of a user's history: every match, with their prediction when there is one
/// (null pred = they didn't predict it, or it's hidden for a non-owner before kickoff).
/// </summary>
public record UserHistoryEntryDto(
    int MatchId,
    string Group,
    TeamDto HomeTeam,
    TeamDto AwayTeam,
    DateTime KickoffUtc,
    string Status,
    MatchResultDto? Result,
    int? PredHomeGoals,
    int? PredAwayGoals,
    int? PointsAwarded);

public record UserHistoryDto(
    string UserId,
    string DisplayName,
    int TotalPoints,
    int ExactHits,
    bool IsOwnerView,
    IReadOnlyList<UserHistoryEntryDto> Predictions);