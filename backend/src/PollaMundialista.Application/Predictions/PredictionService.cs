using Microsoft.EntityFrameworkCore;

using PollaMundialista.Application.Abstractions;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Matches;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Predictions;

public class PredictionService : IPredictionService
{
    private readonly IApplicationDbContext _db;
    private readonly TimeProvider _clock;

    public PredictionService(IApplicationDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<PredictionDto>> GetMineAsync(string userId, CancellationToken ct = default)
    {
        return await _db.Predictions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.MatchId)
            .Select(p => new PredictionDto(p.MatchId, p.PredHomeGoals, p.PredAwayGoals, p.PointsAwarded, p.CreatedAtUtc, p.UpdatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<Result<PredictionDto>> UpsertAsync(string userId, int matchId, UpsertPredictionRequest request, CancellationToken ct = default)
    {
        var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == matchId, ct);
        if (match is null)
            return Result<PredictionDto>.Fail("El partido no existe.", 404);

        var now = _clock.GetUtcNow().UtcDateTime;
        if (match.IsLocked(now))
            return Result<PredictionDto>.Fail("El partido ya inició; no se puede predecir.", 409);

        var prediction = await _db.Predictions.FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == matchId, ct);

        if (prediction is null)
        {
            prediction = new Prediction
            {
                UserId = userId,
                MatchId = matchId,
                PredHomeGoals = request.HomeGoals,
                PredAwayGoals = request.AwayGoals,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            _db.Predictions.Add(prediction);
        }
        else
        {
            prediction.PredHomeGoals = request.HomeGoals;
            prediction.PredAwayGoals = request.AwayGoals;
            prediction.UpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync(ct);

        return Result<PredictionDto>.Ok(new PredictionDto(
            prediction.MatchId, prediction.PredHomeGoals, prediction.PredAwayGoals,
            prediction.PointsAwarded, prediction.CreatedAtUtc, prediction.UpdatedAtUtc));
    }
}