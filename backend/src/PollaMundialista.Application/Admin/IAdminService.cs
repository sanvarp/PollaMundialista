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

    /// <summary>
    /// Reverts a match to Scheduled: clears the result and nulls the points of all
    /// its predictions. Lets an admin undo a result entered by mistake.
    /// </summary>
    Task<Result<MatchDto>> ClearResultAsync(string adminUserId, int matchId, CancellationToken ct = default);
}
