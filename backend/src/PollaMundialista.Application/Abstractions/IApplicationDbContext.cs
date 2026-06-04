using Microsoft.EntityFrameworkCore;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Abstractions;

/// <summary>
/// Data gateway exposed to the Application layer. Implemented by the EF Core
/// AppDbContext in Infrastructure, so application services hold business logic
/// without depending on a concrete persistence type.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Team> Teams { get; }
    DbSet<Match> Matches { get; }
    DbSet<Prediction> Predictions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
