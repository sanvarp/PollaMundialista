namespace PollaMundialista.Domain.Entities;

/// <summary>A national team in the group stage.</summary>
public class Team
{
    public int Id { get; set; }

    /// <summary>3-letter code (FIFA/ISO style), e.g. "ARG".</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Group label: "A", "B", ...</summary>
    public string GroupName { get; set; } = string.Empty;

    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
}
