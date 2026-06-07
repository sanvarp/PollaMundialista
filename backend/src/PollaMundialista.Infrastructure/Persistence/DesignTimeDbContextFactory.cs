using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PollaMundialista.Infrastructure.Persistence;

/// <summary>
/// Used only by the EF Core CLI tools. Forces the SQL Server provider so that
/// generated migrations target the production database (Azure SQL), regardless
/// of the local dev provider (SQLite). The connection string is not used to
/// create migrations, only to select the provider.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=PollaMundialista;Trusted_Connection=True;",
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}