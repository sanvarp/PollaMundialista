using PollaMundialista.Application.Common;

namespace PollaMundialista.Application.Users;

public interface IUserHistoryService
{
    /// <summary>
    /// A user's predictions vs results. Anti-cheat (spec §5.5): when the requester
    /// is not the owner, predictions for matches that have not started are hidden.
    /// </summary>
    Task<Result<UserHistoryDto>> GetAsync(string targetUserId, string requesterUserId, CancellationToken ct = default);
}