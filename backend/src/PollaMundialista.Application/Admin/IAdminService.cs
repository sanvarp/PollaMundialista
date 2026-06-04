using PollaMundialista.Application.Common;
using PollaMundialista.Application.Matches;

namespace PollaMundialista.Application.Admin;

public interface IAdminService
{
    /// <summary>
    /// Stores the final result, marks the match Finished and recalculates points
    /// for every prediction of that match (spec §5.3).
    /// </summary>
    Task<Result<MatchDto>> SetResultAsync(string adminUserId, int matchId, SetResultRequest request, CancellationToken ct = default);
}
