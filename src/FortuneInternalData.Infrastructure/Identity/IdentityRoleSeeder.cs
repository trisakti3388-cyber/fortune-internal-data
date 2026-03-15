using FortuneInternalData.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace FortuneInternalData.Infrastructure.Identity;

public static class IdentityRoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
