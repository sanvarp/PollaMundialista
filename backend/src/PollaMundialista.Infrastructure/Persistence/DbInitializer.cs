using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PollaMundialista.Infrastructure.Identity;

namespace PollaMundialista.Infrastructure.Persistence;

/// <summary>
/// Applies the schema and runs idempotent seeders at startup.
/// SQL Server (Azure) uses EF migrations; SQLite (local dev) uses EnsureCreated
/// so the app runs with zero database setup.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();

        if (db.Database.IsSqlServer())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        await IdentitySeeder.SeedRolesAsync(roleManager);

        // M2 adds domain data seeding (teams, matches, demo users) here.
    }
}
