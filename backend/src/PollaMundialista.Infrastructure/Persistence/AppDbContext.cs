using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Infrastructure.Identity;

namespace PollaMundialista.Infrastructure.Persistence;

/// <summary>
/// EF Core context. Identity tables (AspNetUsers/Roles/...) come from
/// <see cref="IdentityDbContext{TUser}"/>; domain tables are configured via
/// IEntityTypeConfiguration classes in the Configurations folder.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Prediction> Predictions => Set<Prediction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
            b.Property(u => u.DisplayName).HasMaxLength(60).IsRequired());

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
