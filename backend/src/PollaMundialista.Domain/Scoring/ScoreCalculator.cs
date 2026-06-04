namespace PollaMundialista.Domain.Scoring;

/// <summary>
/// Pure scoring rule (see spec §5.1):
///   3 — exact score
///   1 — correct outcome (same sign: home win / draw / away win) but not exact
///   0 — anything else
/// </summary>
public static class ScoreCalculator
{
    public const int ExactScorePoints = 3;
    public const int CorrectOutcomePoints = 1;
    public const int NoPoints = 0;

    public static int Calculate(int actualHome, int actualAway, int predHome, int predAway)
    {
        if (predHome == actualHome && predAway == actualAway)
            return ExactScorePoints;

        if (Math.Sign(predHome - predAway) == Math.Sign(actualHome - actualAway))
            return CorrectOutcomePoints;

        return NoPoints;
    }

    /// <summary>True when the prediction nailed the exact scoreline (used for leaderboard tie-breaks).</summary>
    public static bool IsExactHit(int actualHome, int actualAway, int predHome, int predAway) =>
        predHome == actualHome && predAway == actualAway;
}
