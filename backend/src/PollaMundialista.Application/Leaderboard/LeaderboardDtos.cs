namespace PollaMundialista.Application.Leaderboard;

public record LeaderboardEntryDto(
    int Position,
    string UserId,
    string DisplayName,
    int TotalPoints,
    int ExactHits);
