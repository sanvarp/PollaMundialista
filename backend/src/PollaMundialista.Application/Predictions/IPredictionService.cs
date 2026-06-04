using PollaMundialista.Application.Common;
using PollaMundialista.Application.Matches;

namespace PollaMundialista.Application.Predictions;

public interface IPredictionService
{
    Task<IReadOnlyList<PredictionDto>> GetMineAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Creates or updates the user's prediction for a match. Rejects if the match
    /// has kicked off or is finished (kickoff lock, spec §5.2).
    /// </summary>
    Task<Result<PredictionDto>> UpsertAsync(string userId, int matchId, UpsertPredictionRequest request, CancellationToken ct = default);
}
