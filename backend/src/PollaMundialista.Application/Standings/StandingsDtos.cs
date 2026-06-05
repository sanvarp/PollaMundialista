namespace PollaMundialista.Application.Standings;

/// <summary>One team's row in a group standings table (derived from finished matches).</summary>
public record TeamStandingDto(
    string Code,
    string Name,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);

public record GroupStandingDto(string Group, IReadOnlyList<TeamStandingDto> Rows);
