namespace PollaMundialista.Application.Leaderboard;

public interface ILeaderboardService
{
    /// <summary>Global ranking: TotalPoints desc, ExactHits desc, DisplayName asc (spec §5.4).</summary>
    Task<IReadOnlyList<LeaderboardEntryDto>> GetAsync(CancellationToken ct = default);
}