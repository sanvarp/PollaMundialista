using Microsoft.EntityFrameworkCore;

using PollaMundialista.Application.Abstractions;
using PollaMundialista.Domain.Scoring;

namespace PollaMundialista.Application.Leaderboard;

public class LeaderboardService : ILeaderboardService
{
    private readonly IApplicationDbContext _db;

    public LeaderboardService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetAsync(CancellationToken ct = default)
    {
        // Aggregate scored predictions per user (server-side).
        var aggregates = await _db.Predictions
            .Where(p => p.PointsAwarded != null)
            .GroupBy(p => p.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalPoints = g.Sum(p => p.PointsAwarded!.Value),
                ExactHits = g.Count(p => p.PointsAwarded == ScoreCalculator.ExactScorePoints),
            })
            .ToDictionaryAsync(x => x.UserId, ct);

        var users = await _db.UserSummaries.ToListAsync(ct);

        return users
            .Select(u =>
            {
                var agg = aggregates.GetValueOrDefault(u.Id);
                return (u.Id, u.DisplayName, Total: agg?.TotalPoints ?? 0, Exact: agg?.ExactHits ?? 0);
            })
            .OrderByDescending(x => x.Total)
            .ThenByDescending(x => x.Exact)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select((x, i) => new LeaderboardEntryDto(i + 1, x.Id, x.DisplayName, x.Total, x.Exact))
            .ToList();
    }
}