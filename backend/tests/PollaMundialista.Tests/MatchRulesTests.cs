using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;
using Xunit;

namespace PollaMundialista.Tests;

public class MatchRulesTests
{
    private static Match Scheduled(DateTime kickoffUtc) =>
        new() { KickoffUtc = kickoffUtc, Status = MatchStatus.Scheduled };

    [Fact]
    public void Match_is_open_before_kickoff()
    {
        var now = new DateTime(2026, 6, 11, 18, 0, 0, DateTimeKind.Utc);
        var match = Scheduled(new DateTime(2026, 6, 11, 20, 0, 0, DateTimeKind.Utc));
        Assert.False(match.IsLocked(now));
    }

    [Fact]
    public void Match_is_locked_at_and_after_kickoff()
    {
        var kickoff = new DateTime(2026, 6, 11, 20, 0, 0, DateTimeKind.Utc);
        var match = Scheduled(kickoff);
        Assert.True(match.IsLocked(kickoff));                       // exactly at kickoff
        Assert.True(match.IsLocked(kickoff.AddMinutes(1)));         // after kickoff
    }

    [Fact]
    public void Finished_match_is_locked_even_before_kickoff()
    {
        var now = new DateTime(2026, 6, 11, 18, 0, 0, DateTimeKind.Utc);
        var match = new Match
        {
            KickoffUtc = new DateTime(2026, 6, 11, 20, 0, 0, DateTimeKind.Utc),
            Status = MatchStatus.Finished,
            HomeGoals = 2,
            AwayGoals = 1,
        };
        Assert.True(match.IsLocked(now));
        Assert.True(match.HasResult);
    }

    [Fact]
    public void Scheduled_match_has_no_result()
    {
        Assert.False(Scheduled(DateTime.UtcNow).HasResult);
    }
}
