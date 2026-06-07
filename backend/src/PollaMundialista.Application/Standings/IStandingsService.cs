namespace PollaMundialista.Application.Standings;

public interface IStandingsService
{
    /// <summary>
    /// Group-stage standings derived from finished matches (Pts, GF, GA, GD…).
    /// Pure aggregation — no schema change; demonstrates the relational model scales.
    /// </summary>
    Task<IReadOnlyList<GroupStandingDto>> GetAsync(CancellationToken ct = default);
}