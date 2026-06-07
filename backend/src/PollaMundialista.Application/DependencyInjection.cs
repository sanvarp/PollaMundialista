using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using PollaMundialista.Application.Admin;
using PollaMundialista.Application.Leaderboard;
using PollaMundialista.Application.Matches;
using PollaMundialista.Application.Predictions;
using PollaMundialista.Application.Standings;
using PollaMundialista.Application.Users;

namespace PollaMundialista.Application;

public static class DependencyInjection
{
    /// <summary>Registers application services and FluentValidation validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IUserHistoryService, UserHistoryService>();
        services.AddScoped<IStandingsService, StandingsService>();

        return services;
    }
}