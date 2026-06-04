using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PollaMundialista.Application.Matches;
using PollaMundialista.Application.Predictions;

namespace PollaMundialista.Application;

public static class DependencyInjection
{
    /// <summary>Registers application services and FluentValidation validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IPredictionService, PredictionService>();

        return services;
    }
}
