using FluentValidation;

namespace PollaMundialista.Application.Admin;

public class SetResultRequestValidator : AbstractValidator<SetResultRequest>
{
    private const int MaxGoals = 30;

    public SetResultRequestValidator()
    {
        RuleFor(x => x.HomeGoals).InclusiveBetween(0, MaxGoals)
            .WithMessage($"Los goles deben estar entre 0 y {MaxGoals}.");
        RuleFor(x => x.AwayGoals).InclusiveBetween(0, MaxGoals)
            .WithMessage($"Los goles deben estar entre 0 y {MaxGoals}.");
    }
}
