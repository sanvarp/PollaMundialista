using Microsoft.EntityFrameworkCore;
using PollaMundialista.Application.Abstractions;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Matches;
using PollaMundialista.Domain.Enums;
using PollaMundialista.Domain.Scoring;

namespace PollaMundialista.Application.Admin;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _db;
    private readonly TimeProvider _clock;

    public AdminService(IApplicationDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<MatchDto>> SetResultAsync(string adminUserId, int matchId, SetResultRequest request, CancellationToken ct = default)
    {
        var match = await _db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Predictions)
            .FirstOrDefaultAsync(m => m.Id == matchId, ct);

        if (match is null)
            return Result<MatchDto>.Fail("El partido no existe.", 404);

        var now = _clock.GetUtcNow().UtcDateTime;

        match.HomeGoals = request.HomeGoals;
        match.AwayGoals = request.AwayGoals;
        match.Status = MatchStatus.Finished;

        // Recalculate points for every prediction of this match.
        foreach (var prediction in match.Predictions)
        {
            prediction.PointsAwarded = ScoreCalculator.Calculate(
                request.HomeGoals, request.AwayGoals,
                prediction.PredHomeGoals, prediction.PredAwayGoals);
            prediction.UpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync(ct);

        var mine = match.Predictions.FirstOrDefault(p => p.UserId == adminUserId);
        return Result<MatchDto>.Ok(MatchService.MapToDto(match, now, mine));
    }
}
