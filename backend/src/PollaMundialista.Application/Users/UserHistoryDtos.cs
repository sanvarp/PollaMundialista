using PollaMundialista.Application.Matches;

namespace PollaMundialista.Application.Users;

/// <summary>One row of a user's prediction history (match + their prediction + result).</summary>
public record UserHistoryEntryDto(
    int MatchId,
    string Group,
    TeamDto HomeTeam,
    TeamDto AwayTeam,
    DateTime KickoffUtc,
    string Status,
    MatchResultDto? Result,
    int PredHomeGoals,
    int PredAwayGoals,
    int? PointsAwarded);

public record UserHistoryDto(
    string UserId,
    string DisplayName,
    int TotalPoints,
    int ExactHits,
    bool IsOwnerView,
    IReadOnlyList<UserHistoryEntryDto> Predictions);