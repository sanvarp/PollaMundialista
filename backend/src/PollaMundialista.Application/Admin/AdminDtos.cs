namespace PollaMundialista.Application.Admin;

/// <summary>Final score entered by an admin for a match.</summary>
public record SetResultRequest(int HomeGoals, int AwayGoals);