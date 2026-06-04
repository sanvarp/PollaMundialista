using FluentValidation;

namespace PollaMundialista.Application.Predictions;

public class UpsertPredictionRequestValidator : AbstractValidator<UpsertPredictionRequest>
{
    // A sane upper bound; goals can never be negative.
    private const int MaxGoals = 30;

    public UpsertPredictionRequestValidator()
    {
        RuleFor(x => x.HomeGoals)
            .InclusiveBetween(0, MaxGoals)
            .WithMessage($"Los goles deben estar entre 0 y {MaxGoals}.");

        RuleFor(x => x.AwayGoals)
            .InclusiveBetween(0, MaxGoals)
            .WithMessage($"Los goles deben estar entre 0 y {MaxGoals}.");
    }
}
