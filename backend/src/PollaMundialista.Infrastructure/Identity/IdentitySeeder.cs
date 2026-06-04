using Microsoft.AspNetCore.Identity;
using PollaMundialista.Application.Common;

namespace PollaMundialista.Infrastructure.Identity;

/// <summary>Seeds the Admin/User roles. Idempotent.</summary>
public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
