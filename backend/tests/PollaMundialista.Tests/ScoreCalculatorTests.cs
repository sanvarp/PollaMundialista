using PollaMundialista.Domain.Scoring;
using Xunit;

namespace PollaMundialista.Tests;

public class ScoreCalculatorTests
{
    [Theory]
    // Exact score => 3 (spec §5.1 examples)
    [InlineData(2, 1, 2, 1, 3)]
    [InlineData(0, 0, 0, 0, 3)]
    [InlineData(0, 2, 0, 2, 3)]
    // Correct outcome, not exact => 1
    [InlineData(2, 1, 3, 0, 1)] // home win predicted as home win
    [InlineData(1, 1, 0, 0, 1)] // draw predicted as draw
    [InlineData(0, 2, 1, 3, 1)] // away win predicted as away win
    // Wrong outcome => 0
    [InlineData(2, 1, 1, 1, 0)] // home win vs predicted draw
    [InlineData(2, 1, 0, 3, 0)] // home win vs predicted away win
    [InlineData(1, 1, 2, 1, 0)] // draw vs predicted home win
    public void Calculate_returns_expected_points(int aH, int aA, int pH, int pA, int expected)
    {
        Assert.Equal(expected, ScoreCalculator.Calculate(aH, aA, pH, pA));
    }

    [Theory]
    [InlineData(2, 1, 2, 1, true)]
    [InlineData(2, 1, 1, 1, false)]
    public void IsExactHit_detects_exact_scoreline(int aH, int aA, int pH, int pA, bool expected)
    {
        Assert.Equal(expected, ScoreCalculator.IsExactHit(aH, aA, pH, pA));
    }
}
