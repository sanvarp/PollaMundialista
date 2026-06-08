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

        var predsByMatch = (await _db.Predictions
                .AsNoTracking()
                .Where(p => p.UserId == targetUserId)
                .ToListAsync(ct))
            .ToDictionary(p => p.MatchId);

        // Aggregates use ALL scored predictions (numbers leak nothing; keeps parity with leaderboard).
        var totalPoints = predsByMatch.Values.Where(p => p.PointsAwarded.HasValue).Sum(p => p.PointsAwarded!.Value);
        var exactHits = predsByMatch.Values.Count(p => p.PointsAwarded == ScoreCalculator.ExactScorePoints);

        // Show EVERY match (so "didn't predict / 0 points" is visible too), with the user's
        // prediction when there is one. Anti-cheat: a non-owner doesn't see a prediction for
        // a match that hasn't started yet.
        var matches = await _db.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync(ct);

        var entries = matches.Select(m =>
        {
            predsByMatch.TryGetValue(m.Id, out var p);
            var showPred = p is not null && (isOwner || m.IsLocked(now));
            return new UserHistoryEntryDto(
                m.Id,
                m.GroupName,
                new TeamDto(m.HomeTeam!.Code, m.HomeTeam.Name, m.HomeTeam.GroupName),
                new TeamDto(m.AwayTeam!.Code, m.AwayTeam.Name, m.AwayTeam.GroupName),
                m.KickoffUtc,
                m.Status.ToString(),
                m.HasResult ? new MatchResultDto(m.HomeGoals!.Value, m.AwayGoals!.Value) : null,
                showPred ? p!.PredHomeGoals : null,
                showPred ? p!.PredAwayGoals : null,
                showPred ? p!.PointsAwarded : null);
        }).ToList();

        return Result<UserHistoryDto>.Ok(new UserHistoryDto(
            user.Id, user.DisplayName, totalPoints, exactHits, isOwner, entries));
    }
}