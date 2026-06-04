using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PollaMundialista.Application.Admin;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;
using PollaMundialista.Infrastructure.Identity;
using PollaMundialista.Infrastructure.Persistence;
using Xunit;

namespace PollaMundialista.Tests;

/// <summary>
/// Exercises the result-entry recalculation against a real EF model (in-memory SQLite).
/// </summary>
public class AdminServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;

    public AdminServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task SetResult_marks_finished_and_recalculates_points()
    {
        // Arrange: a match with three predictions (exact / outcome / wrong).
        var home = new Team { Code = "ARG", Name = "Argentina", GroupName = "B" };
        var away = new Team { Code = "ESP", Name = "España", GroupName = "B" };
        _db.Teams.AddRange(home, away);
        await _db.SaveChangesAsync();

        var match = new Match
        {
            GroupName = "B",
            HomeTeamId = home.Id,
            AwayTeamId = away.Id,
            KickoffUtc = new DateTime(2026, 6, 13, 18, 0, 0, DateTimeKind.Utc),
            Status = MatchStatus.Scheduled,
        };
        _db.Matches.Add(match);

        // Predictions FK to AspNetUsers, so the users must exist.
        _db.Users.AddRange(
            new ApplicationUser { Id = "u-exact", UserName = "exact", DisplayName = "Exact" },
            new ApplicationUser { Id = "u-outcome", UserName = "outcome", DisplayName = "Outcome" },
            new ApplicationUser { Id = "u-wrong", UserName = "wrong", DisplayName = "Wrong" });
        await _db.SaveChangesAsync();

        _db.Predictions.AddRange(
            new Prediction { UserId = "u-exact", MatchId = match.Id, PredHomeGoals = 3, PredAwayGoals = 1 },
            new Prediction { UserId = "u-outcome", MatchId = match.Id, PredHomeGoals = 2, PredAwayGoals = 0 },
            new Prediction { UserId = "u-wrong", MatchId = match.Id, PredHomeGoals = 0, PredAwayGoals = 2 });
        await _db.SaveChangesAsync();

        var service = new AdminService(_db, TimeProvider.System);

        // Act: real result 3-1 (home win).
        var result = await service.SetResultAsync("admin", match.Id, new SetResultRequest(3, 1));

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Finished", result.Value!.Status);
        Assert.Equal(3, result.Value.Result!.HomeGoals);

        var points = await _db.Predictions.ToDictionaryAsync(p => p.UserId, p => p.PointsAwarded);
        Assert.Equal(3, points["u-exact"]);   // exact score
        Assert.Equal(1, points["u-outcome"]); // correct winner, wrong score
        Assert.Equal(0, points["u-wrong"]);   // wrong outcome
    }

    [Fact]
    public async Task SetResult_returns_404_for_unknown_match()
    {
        var service = new AdminService(_db, TimeProvider.System);
        var result = await service.SetResultAsync("admin", 999, new SetResultRequest(1, 0));
        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
