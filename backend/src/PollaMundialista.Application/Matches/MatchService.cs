using Microsoft.EntityFrameworkCore;
using PollaMundialista.Application.Abstractions;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Matches;

public class MatchService : IMatchService
{
    private readonly IApplicationDbContext _db;
    private readonly TimeProvider _clock;

    public MatchService(IApplicationDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<MatchDto>> GetAllAsync(string userId, CancellationToken ct = default)
    {
        var matches = await _db.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync(ct);

        var myPredictions = await _db.Predictions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.MatchId, ct);

        var now = _clock.GetUtcNow().UtcDateTime;

        return matches.Select(m => MapToDto(m, now, myPredictions.GetValueOrDefault(m.Id))).ToList();
    }

    internal static MatchDto MapToDto(Match m, DateTime nowUtc, Prediction? myPrediction) => new(
        m.Id,
        m.GroupName,
        new TeamDto(m.HomeTeam!.Code, m.HomeTeam.Name, m.HomeTeam.GroupName),
        new TeamDto(m.AwayTeam!.Code, m.AwayTeam.Name, m.AwayTeam.GroupName),
        m.KickoffUtc,
        m.Status.ToString(),
        m.IsLocked(nowUtc),
        m.HasResult ? new MatchResultDto(m.HomeGoals!.Value, m.AwayGoals!.Value) : null,
        myPrediction is null
            ? null
            : new PredictionDto(myPrediction.MatchId, myPrediction.PredHomeGoals, myPrediction.PredAwayGoals,
                myPrediction.PointsAwarded, myPrediction.CreatedAtUtc, myPrediction.UpdatedAtUtc));
}
