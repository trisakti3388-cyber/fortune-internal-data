using FortuneInternalData.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace FortuneInternalData.Infrastructure.Identity;

public class IdentityBootstrapService
{
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public IdentityBootstrapService(UserManager<IdentityApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task EnsureSuperadminAsync()
    {
        var email = _configuration["Bootstrap:SuperadminEmail"];
        var password = _configuration["Bootstrap:SuperadminPassword"];
        var name = _configuration["Bootstrap:SuperadminName"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return;

        var user = new IdentityApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = name,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Roles.Superadmin);
        }
    }
}
