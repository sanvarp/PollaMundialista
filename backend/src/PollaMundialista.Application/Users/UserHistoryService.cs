using Microsoft.EntityFrameworkCore;

using PollaMundialista.Application.Abstractions;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Matches;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Scoring;

namespace PollaMundialista.Application.Users;

public class UserHistoryService : IUserHistoryService
{
    private readonly IApplicationDbContext _db;
    private readonly TimeProvider _clock;

    public UserHistoryService(IApplicationDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<UserHistoryDto>> GetAsync(string targetUserId, string requesterUserId, CancellationToken ct = default)
    {
        // Filtering the projected UserSummaries query isn't translatable; the user set is
        // tiny, so materialize and match in memory.
        var users = await _db.UserSummaries.ToListAsync(ct);
        var user = users.FirstOrDefault(u => u.Id == targetUserId);
        if (user is null)
            return Result<UserHistoryDto>.Fail("El usuario no existe.", 404);

        var isOwner = string.Equals(targetUserId, requesterUserId, StringComparison.Ordinal);
        var now = _clock.GetUtcNow().UtcDateTime;

        var predictions = await _db.Predictions
            .AsNoTracking()
            .Where(p => p.UserId == targetUserId)
            .Include(p => p.Match!).ThenInclude(m => m.HomeTeam)
            .Include(p => p.Match!).ThenInclude(m => m.AwayTeam)
            .ToListAsync(ct);

        // Aggregates use ALL scored predictions (numbers leak nothing; keeps parity with leaderboard).
        var totalPoints = predictions.Where(p => p.PointsAwarded.HasValue).Sum(p => p.PointsAwarded!.Value);
        var exactHits = predictions.Count(p => p.PointsAwarded == ScoreCalculator.ExactScorePoints);

        // Anti-cheat: non-owners only see predictions for matches that have started.
        var visible = predictions
            .Where(p => isOwner || p.Match!.IsLocked(now))
            .OrderBy(p => p.Match!.KickoffUtc)
            .Select(MapEntry)
            .ToList();

        return Result<UserHistoryDto>.Ok(new UserHistoryDto(
            user.Id, user.DisplayName, totalPoints, exactHits, isOwner, visible));
    }

    private static UserHistoryEntryDto MapEntry(Prediction p)
    {
        var m = p.Match!;
        return new UserHistoryEntryDto(
            m.Id,
            m.GroupName,
            new TeamDto(m.HomeTeam!.Code, m.HomeTeam.Name, m.HomeTeam.GroupName),
            new TeamDto(m.AwayTeam!.Code, m.AwayTeam.Name, m.AwayTeam.GroupName),
            m.KickoffUtc,
            m.Status.ToString(),
            m.HasResult ? new MatchResultDto(m.HomeGoals!.Value, m.AwayGoals!.Value) : null,
            p.PredHomeGoals,
            p.PredAwayGoals,
            p.PointsAwarded);
    }
}