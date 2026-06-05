using Microsoft.EntityFrameworkCore;
using PollaMundialista.Application.Abstractions;
using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Application.Standings;

public class StandingsService : IStandingsService
{
    private readonly IApplicationDbContext _db;

    public StandingsService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<GroupStandingDto>> GetAsync(CancellationToken ct = default)
    {
        var teams = await _db.Teams.AsNoTracking().ToListAsync(ct);
        var finished = await _db.Matches
            .AsNoTracking()
            .Where(m => m.Status == MatchStatus.Finished && m.HomeGoals != null && m.AwayGoals != null)
            .ToListAsync(ct);

        // Accumulate per-team stats from each finished match.
        var stats = teams.ToDictionary(t => t.Id, t => new Acc(t.Code, t.Name, t.GroupName));

        foreach (var m in finished)
        {
            var home = stats[m.HomeTeamId];
            var away = stats[m.AwayTeamId];
            home.Apply(m.HomeGoals!.Value, m.AwayGoals!.Value);
            away.Apply(m.AwayGoals!.Value, m.HomeGoals!.Value);
        }

        return stats.Values
            .GroupBy(a => a.Group)
            .OrderBy(g => g.Key)
            .Select(g => new GroupStandingDto(
                g.Key,
                g.OrderByDescending(a => a.Points)
                    .ThenByDescending(a => a.GoalsFor - a.GoalsAgainst)
                    .ThenByDescending(a => a.GoalsFor)
                    .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(a => a.ToDto())
                    .ToList()))
            .ToList();
    }

    /// <summary>Mutable per-team accumulator used while folding over matches.</summary>
    private sealed class Acc(string code, string name, string group)
    {
        public string Group { get; } = group;
        public string Name { get; } = name;
        public int Played, Won, Drawn, Lost, GoalsFor, GoalsAgainst, Points;

        public void Apply(int scored, int conceded)
        {
            Played++;
            GoalsFor += scored;
            GoalsAgainst += conceded;
            if (scored > conceded) { Won++; Points += 3; }
            else if (scored == conceded) { Drawn++; Points += 1; }
            else Lost++;
        }

        public TeamStandingDto ToDto() =>
            new(code, Name, Played, Won, Drawn, Lost, GoalsFor, GoalsAgainst, GoalsFor - GoalsAgainst, Points);
    }
}
