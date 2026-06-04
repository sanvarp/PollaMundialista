namespace PollaMundialista.Application.Matches;

public record TeamDto(string Code, string Name, string GroupName);

public record MatchResultDto(int HomeGoals, int AwayGoals);

public record PredictionDto(
    int MatchId,
    int HomeGoals,
    int AwayGoals,
    int? PointsAwarded,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Match plus, for the current user, their prediction and (if played) the result.</summary>
public record MatchDto(
    int Id,
    string Group,
    TeamDto HomeTeam,
    TeamDto AwayTeam,
    DateTime KickoffUtc,
    string Status,
    bool IsLocked,
    MatchResultDto? Result,
    PredictionDto? MyPrediction);
