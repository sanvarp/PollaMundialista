using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PollaMundialista.Infrastructure.Identity;

namespace PollaMundialista.Infrastructure.Persistence;

/// <summary>
/// EF Core context. Identity tables (AspNetUsers/Roles/...) come from
/// <see cref="IdentityDbContext{TUser}"/>. Domain DbSets (Teams/Matches/Predictions)
/// are added in M2.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.DisplayName).HasMaxLength(60).IsRequired();
        });
    }
}
