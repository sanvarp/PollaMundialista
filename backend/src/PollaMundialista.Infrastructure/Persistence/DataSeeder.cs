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
/// Idempotent seeding of teams, the 12 group-stage matches, demo users and
/// sample predictions. Demo realism: matchday 1 of each group is pre-loaded as
/// played so the leaderboard is populated; the rest stay open for live prediction.
/// </summary>
public static class DataSeeder
{
    // --- Teams (real national teams; group draw is illustrative for the demo) ---
    private static readonly (string Code, string Name, string Group)[] TeamData =
    [
        ("MEX", "México", "A"), ("CRO", "Croacia", "A"), ("JPN", "Japón", "A"), ("GHA", "Ghana", "A"),
        ("ARG", "Argentina", "B"), ("ESP", "España", "B"), ("MAR", "Marruecos", "B"), ("KOR", "Corea del Sur", "B"),
    ];

    // Round-robin fixtures. Goals != null => pre-loaded as Finished for the demo.
    private static readonly FixtureSeed[] Fixtures =
    [
        // Group A
        new("A", "MEX", "CRO", "2026-06-11T20:00:00Z", 2, 1),
        new("A", "JPN", "GHA", "2026-06-12T18:00:00Z", 0, 0),
        new("A", "MEX", "JPN", "2026-06-16T21:00:00Z", null, null),
        new("A", "CRO", "GHA", "2026-06-17T18:00:00Z", null, null),
        new("A", "MEX", "GHA", "2026-06-21T20:00:00Z", null, null),
        new("A", "CRO", "JPN", "2026-06-21T23:00:00Z", null, null),
        // Group B
        new("B", "ARG", "ESP", "2026-06-13T18:00:00Z", 3, 1),
        new("B", "MAR", "KOR", "2026-06-14T21:00:00Z", 1, 1),
        new("B", "ARG", "MAR", "2026-06-18T18:00:00Z", null, null),
        new("B", "ESP", "KOR", "2026-06-19T21:00:00Z", null, null),
        new("B", "ARG", "KOR", "2026-06-24T20:00:00Z", null, null),
        new("B", "ESP", "MAR", "2026-06-24T23:00:00Z", null, null),
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
        ["user@polla.com"] =
        [
            ("A", "MEX", "CRO", 2, 1), ("A", "JPN", "GHA", 1, 0),
            ("B", "ARG", "ESP", 2, 0), ("B", "MAR", "KOR", 1, 1),
            ("A", "MEX", "JPN", 2, 0), ("B", "ARG", "MAR", 2, 1),
        ],
        ["ana@polla.com"] =
        [
            ("A", "MEX", "CRO", 1, 0), ("A", "JPN", "GHA", 0, 0),
            ("B", "ARG", "ESP", 3, 1), ("B", "MAR", "KOR", 2, 2),
        ],
        ["carlos@polla.com"] =
        [
            ("A", "MEX", "CRO", 2, 1), ("A", "JPN", "GHA", 0, 0),
            ("B", "ARG", "ESP", 1, 0), ("B", "MAR", "KOR", 0, 1),
        ],
        ["valentina@polla.com"] =
        [
            ("A", "MEX", "CRO", 0, 2), ("A", "JPN", "GHA", 1, 1),
            ("B", "ARG", "ESP", 2, 1), ("B", "MAR", "KOR", 1, 1),
        ],
        ["admin@polla.com"] =
        [
            ("A", "MEX", "CRO", 3, 2), ("B", "ARG", "ESP", 3, 1),
        ],
    };

    public static async Task SeedAsync(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        await SeedTeamsAsync(db);
        await SeedMatchesAsync(db);
        var userIds = await SeedUsersAsync(userManager);
        await SeedPredictionsAsync(db, userIds);
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