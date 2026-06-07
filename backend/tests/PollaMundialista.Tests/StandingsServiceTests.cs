using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PollaMundialista.Application.Standings;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;
using PollaMundialista.Infrastructure.Persistence;

using Xunit;

namespace PollaMundialista.Tests;

public class StandingsServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;

    public StandingsServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Standings_compute_points_and_order_correctly()
    {
        var t1 = new Team { Code = "T1", Name = "Team 1", GroupName = "A" };
        var t2 = new Team { Code = "T2", Name = "Team 2", GroupName = "A" };
        var t3 = new Team { Code = "T3", Name = "Team 3", GroupName = "A" };
        var t4 = new Team { Code = "T4", Name = "Team 4", GroupName = "A" };
        _db.Teams.AddRange(t1, t2, t3, t4);
        await _db.SaveChangesAsync();

        static Match Played(Team h, Team a, int hg, int ag) => new()
        {
            GroupName = "A",
            HomeTeamId = h.Id,
            AwayTeamId = a.Id,
            KickoffUtc = new DateTime(2026, 6, 11, 20, 0, 0, DateTimeKind.Utc),
            Status = MatchStatus.Finished,
            HomeGoals = hg,
            AwayGoals = ag,
        };

        _db.Matches.AddRange(
            Played(t1, t2, 2, 0), // T1 win
            Played(t3, t4, 1, 1), // draw
            Played(t1, t3, 1, 0), // T1 win
            Played(t2, t4, 0, 2)); // T4 win
        // An unfinished match must not count.
        _db.Matches.Add(new Match { GroupName = "A", HomeTeamId = t2.Id, AwayTeamId = t3.Id, KickoffUtc = DateTime.UtcNow, Status = MatchStatus.Scheduled });
        await _db.SaveChangesAsync();

        var groups = await new StandingsService(_db).GetAsync();

        var rows = Assert.Single(groups).Rows;
        Assert.Equal(new[] { "T1", "T4", "T3", "T2" }, rows.Select(r => r.Code));

        var top = rows[0];
        Assert.Equal(6, top.Points);
        Assert.Equal(2, top.Played);
        Assert.Equal(2, top.Won);
        Assert.Equal(3, top.GoalsFor);
        Assert.Equal(0, top.GoalsAgainst);
        Assert.Equal(3, top.GoalDifference);

        Assert.Equal(4, rows[1].Points); // T4: win + draw
        Assert.Equal(1, rows[2].Points); // T3: draw + loss
        Assert.Equal(0, rows[3].Points); // T2: two losses
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}