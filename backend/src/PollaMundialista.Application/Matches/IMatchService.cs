namespace PollaMundialista.Application.Matches;

public interface IMatchService
{
    /// <summary>All matches (ordered by kickoff) with the current user's prediction and result if played.</summary>
    Task<IReadOnlyList<MatchDto>> GetAllAsync(string userId, CancellationToken ct = default);
}