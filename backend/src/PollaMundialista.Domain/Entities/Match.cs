using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Domain.Entities;

/// <summary>A group-stage fixture. Goals are null until a result is entered.</summary>
public class Match
{
    public int Id { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public int HomeTeamId { get; set; }
    public Team? HomeTeam { get; set; }

    public int AwayTeamId { get; set; }
    public Team? AwayTeam { get; set; }

    /// <summary>Kickoff stored in UTC; the client formats to local time.</summary>
    public DateTime KickoffUtc { get; set; }

    public string Stage { get; set; } = "Group";

    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();

    /// <summary>True once the match has a final result.</summary>
    public bool HasResult => Status == MatchStatus.Finished && HomeGoals.HasValue && AwayGoals.HasValue;

    /// <summary>
    /// A prediction is locked once the match has kicked off OR it is finished.
    /// </summary>
    public bool IsLocked(DateTime nowUtc) => Status == MatchStatus.Finished || nowUtc >= KickoffUtc;
}
