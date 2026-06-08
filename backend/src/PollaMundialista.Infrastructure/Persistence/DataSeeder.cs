using System.Globalization;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PollaMundialista.Application.Common;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;
using PollaMundialista.Domain.Scoring;
using PollaMundialista.Infrastructure.Identity;

namespace PollaMundialista.Infrastructure.Persistence;

/// <summary>
/// Idempotent seeding of teams, the 12 group-stage matches, demo users and sample
/// predictions. Uses the REAL Group A and Group B of the 2026 FIFA World Cup (teams,
/// fixtures and kickoff dates/times in UTC). Demo device: matchday 1 of each group is
/// pre-loaded as played (illustrative scores) so the leaderboard is populated; the
/// rest stay open for live prediction.
/// </summary>
public static class DataSeeder
{
    // --- Real Group A and Group B of the 2026 FIFA World Cup ---
    private static readonly (string Code, string Name, string Group)[] TeamData =
    [
        ("MEX", "México", "A"), ("RSA", "Sudáfrica", "A"), ("KOR", "Corea del Sur", "A"), ("CZE", "República Checa", "A"),
        ("CAN", "Canadá", "B"), ("BIH", "Bosnia y Herzegovina", "B"), ("QAT", "Catar", "B"), ("SUI", "Suiza", "B"),
    ];

    // Real fixtures (kickoff in UTC). Goals != null => matchday 1, pre-loaded as Finished
    // with illustrative scores so the leaderboard is populated; matchdays 2 and 3 stay open.
    private static readonly FixtureSeed[] Fixtures =
    [
        // Group A
        new("A", "MEX", "RSA", "2026-06-11T19:00:00Z", 2, 0),
        new("A", "KOR", "CZE", "2026-06-12T02:00:00Z", 1, 1),
        new("A", "CZE", "RSA", "2026-06-18T16:00:00Z", null, null),
        new("A", "MEX", "KOR", "2026-06-19T01:00:00Z", null, null),
        new("A", "CZE", "MEX", "2026-06-25T01:00:00Z", null, null),
        new("A", "RSA", "KOR", "2026-06-25T01:00:00Z", null, null),
        // Group B
        new("B", "CAN", "BIH", "2026-06-12T19:00:00Z", 1, 0),
        new("B", "QAT", "SUI", "2026-06-13T19:00:00Z", 0, 2),
        new("B", "SUI", "BIH", "2026-06-18T19:00:00Z", null, null),
        new("B", "CAN", "QAT", "2026-06-18T22:00:00Z", null, null),
        new("B", "SUI", "CAN", "2026-06-24T19:00:00Z", null, null),
        new("B", "BIH", "QAT", "2026-06-24T19:00:00Z", null, null),
    ];

    private static readonly DemoUser[] DemoUsers =
    [
        new("admin@polla.com", "Administrador", "Admin#2026", Roles.Admin),
        new("user@polla.com", "Jugador Demo", "User#2026", Roles.User),
        new("ana@polla.com", "Ana Restrepo", "User#2026", Roles.User),
        new("carlos@polla.com", "Carlos Mejía", "User#2026", Roles.User),
        new("valentina@polla.com", "Valentina Ríos", "User#2026", Roles.User),
    ];

    // Sample predictions per user: (group, home, away, predHome, predAway).
    private static readonly Dictionary<string, (string G, string H, string A, int Ph, int Pa)[]> PredictionData = new()
    {
        // Mix of played + open, leaving some matches unpredicted on purpose so the history
        // shows "No predijo" / 0 points too (e.g. user@polla didn't predict QAT–SUI).
        ["user@polla.com"] =
        [
            ("A", "MEX", "RSA", 2, 0), ("A", "KOR", "CZE", 1, 1),
            ("B", "CAN", "BIH", 2, 0),
            ("A", "MEX", "KOR", 2, 1), ("B", "CAN", "QAT", 1, 0),
        ],
        ["ana@polla.com"] =
        [
            ("A", "MEX", "RSA", 2, 0), ("A", "KOR", "CZE", 1, 1),
            ("B", "CAN", "BIH", 1, 0), ("B", "QAT", "SUI", 0, 1),
        ],
        ["carlos@polla.com"] =
        [
            ("A", "MEX", "RSA", 1, 0), ("A", "KOR", "CZE", 0, 0),
            ("B", "CAN", "BIH", 1, 0), ("B", "QAT", "SUI", 0, 2),
        ],
        ["valentina@polla.com"] =
        [
            ("A", "MEX", "RSA", 0, 1), ("B", "CAN", "BIH", 1, 0),
            ("A", "MEX", "KOR", 1, 1), ("B", "SUI", "BIH", 2, 0),
        ],
        ["admin@polla.com"] =
        [
            ("A", "MEX", "RSA", 3, 1), ("B", "QAT", "SUI", 0, 2),
        ],
    };

    public static async Task SeedAsync(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        await ReplaceStaleFixturesAsync(db);
        await SeedTeamsAsync(db);
        await SeedMatchesAsync(db);
        var userIds = await SeedUsersAsync(userManager);
        await SeedPredictionsAsync(db, userIds);
    }

    /// <summary>
    /// If the database was seeded with a different set of teams (e.g. an older illustrative
    /// draw on an already-deployed environment), wipe teams/matches/predictions so the
    /// idempotent seeders below re-create them with the current real fixtures. Triggers ONLY
    /// when the team set changes — an admin entering a result never matches, so live results
    /// are preserved across restarts.
    /// </summary>
    private static async Task ReplaceStaleFixturesAsync(AppDbContext db)
    {
        if (!await db.Teams.AnyAsync())
            return;

        var expected = TeamData.Select(t => t.Code).ToHashSet();
        var current = (await db.Teams.Select(t => t.Code).ToListAsync()).ToHashSet();
        if (expected.SetEquals(current))
            return;

        // Wipe in FK order (predictions -> matches -> teams), in one transaction so a crash
        // mid-way can't leave a half-cleared schema (it would self-heal next start anyway).
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Predictions.ExecuteDeleteAsync();
        await db.Matches.ExecuteDeleteAsync();
        await db.Teams.ExecuteDeleteAsync();
        await tx.CommitAsync();
    }

    private static async Task SeedTeamsAsync(AppDbContext db)
    {
        if (await db.Teams.AnyAsync()) return;
        db.Teams.AddRange(TeamData.Select(t => new Team { Code = t.Code, Name = t.Name, GroupName = t.Group }));
        await db.SaveChangesAsync();
    }

    private static async Task SeedMatchesAsync(AppDbContext db)
    {
        if (await db.Matches.AnyAsync()) return;
        var teams = await db.Teams.ToDictionaryAsync(t => t.Code, t => t.Id);

        foreach (var f in Fixtures)
        {
            var finished = f.HomeGoals.HasValue && f.AwayGoals.HasValue;
            db.Matches.Add(new Match
            {
                GroupName = f.Group,
                HomeTeamId = teams[f.HomeCode],
                AwayTeamId = teams[f.AwayCode],
                KickoffUtc = DateTime.Parse(f.KickoffUtc, CultureInfo.InvariantCulture).ToUniversalTime(),
                Stage = "Group",
                HomeGoals = f.HomeGoals,
                AwayGoals = f.AwayGoals,
                Status = finished ? MatchStatus.Finished : MatchStatus.Scheduled,
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, string>> SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var ids = new Dictionary<string, string>();
        foreach (var u in DemoUsers)
        {
            var user = await userManager.FindByEmailAsync(u.Email);
            if (user is null)
            {
                user = new ApplicationUser { UserName = u.Email, Email = u.Email, DisplayName = u.DisplayName, EmailConfirmed = true };
                await userManager.CreateAsync(user, u.Password);
                await userManager.AddToRoleAsync(user, u.Role);
            }
            ids[u.Email] = user.Id;
        }
        return ids;
    }

    private static async Task SeedPredictionsAsync(AppDbContext db, Dictionary<string, string> userIds)
    {
        if (await db.Predictions.AnyAsync()) return;

        var matches = await db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .ToListAsync();

        Match? Find(string g, string h, string a) =>
            matches.FirstOrDefault(m => m.GroupName == g && m.HomeTeam!.Code == h && m.AwayTeam!.Code == a);

        var now = DateTime.UtcNow;

        foreach (var (email, preds) in PredictionData)
        {
            if (!userIds.TryGetValue(email, out var userId)) continue;

            foreach (var p in preds)
            {
                var match = Find(p.G, p.H, p.A);
                if (match is null) continue;

                int? points = match.HasResult
                    ? ScoreCalculator.Calculate(match.HomeGoals!.Value, match.AwayGoals!.Value, p.Ph, p.Pa)
                    : null;

                db.Predictions.Add(new Prediction
                {
                    UserId = userId,
                    MatchId = match.Id,
                    PredHomeGoals = p.Ph,
                    PredAwayGoals = p.Pa,
                    PointsAwarded = points,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                });
            }
        }
        await db.SaveChangesAsync();
    }

    private sealed record FixtureSeed(string Group, string HomeCode, string AwayCode, string KickoffUtc, int? HomeGoals, int? AwayGoals);
    private sealed record DemoUser(string Email, string DisplayName, string Password, string Role);
}