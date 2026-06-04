using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollaMundialista.Application.Abstractions;
using PollaMundialista.Application.Common;
using PollaMundialista.Infrastructure.Identity;
using PollaMundialista.Infrastructure.Persistence;

namespace PollaMundialista.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // --- Database (provider-agnostic) ---------------------------------
        // Local dev defaults to SQLite (zero install). Production uses Azure SQL.
        var provider = config.GetValue<string>("Database:Provider") ?? "Sqlite";
        var connectionString = config.GetConnectionString("Default")
            ?? (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase)
                ? "Data Source=polla.db"
                : throw new InvalidOperationException("ConnectionStrings:Default is required for SqlServer."));

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            else
                options.UseSqlite(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton(TimeProvider.System);

        // --- Identity ------------------------------------------------------
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        // --- JWT settings --------------------------------------------------
        services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));

        // --- Application service implementations ---------------------------
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
