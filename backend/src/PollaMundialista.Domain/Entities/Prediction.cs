namespace PollaMundialista.Domain.Entities;

/// <summary>A user's predicted score for a match. One per (user, match) — enforced by a unique index.</summary>
public class Prediction
{
    public int Id { get; set; }

    /// <summary>FK to AspNetUsers.Id (Identity user); kept as a string so Domain stays Identity-free.</summary>
    public string UserId { get; set; } = string.Empty;

    public int MatchId { get; set; }
    public Match? Match { get; set; }

    public int PredHomeGoals { get; set; }
    public int PredAwayGoals { get; set; }

    /// <summary>Null until the match result is entered and points are computed.</summary>
    public int? PointsAwarded { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
