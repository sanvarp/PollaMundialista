using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace PollaMundialista.Application;

public static class DependencyInjection
{
    /// <summary>Registers FluentValidation validators (and future application services).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
