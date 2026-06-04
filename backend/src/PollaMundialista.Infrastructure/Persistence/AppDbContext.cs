using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PollaMundialista.Application.Abstractions;
using PollaMundialista.Application.Common;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Infrastructure.Identity;

namespace PollaMundialista.Infrastructure.Persistence;

/// <summary>
/// EF Core context. Identity tables (AspNetUsers/Roles/...) come from
/// <see cref="IdentityDbContext{TUser}"/>; domain tables are configured via
/// IEntityTypeConfiguration classes in the Configurations folder.
/// Implements <see cref="IApplicationDbContext"/> so the Application layer
/// can access data without a concrete persistence dependency.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Prediction> Predictions => Set<Prediction>();

    // Projects Identity users to the Application-facing summary (composes server-side).
    public IQueryable<UserSummary> UserSummaries =>
        Users.Select(u => new UserSummary(u.Id, u.DisplayName));

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
            b.Property(u => u.DisplayName).HasMaxLength(60).IsRequired());

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ApplyUtcDateTimeConverter(builder);
    }

    /// <summary>
    /// Forces every DateTime to round-trip as UTC. SQLite does not preserve
    /// DateTimeKind, so without this, timestamps would deserialize as 'Unspecified'
    /// and the client would treat them as local time.
    /// </summary>
    private static void ApplyUtcDateTimeConverter(ModelBuilder builder)
    {
        var utc = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var utcNullable = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utc);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(utcNullable);
            }
        }
    }
}
