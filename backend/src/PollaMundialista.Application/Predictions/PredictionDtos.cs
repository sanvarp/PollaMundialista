namespace PollaMundialista.Application.Predictions;

/// <summary>Payload to create or update a prediction for a match.</summary>
public record UpsertPredictionRequest(int HomeGoals, int AwayGoals);
