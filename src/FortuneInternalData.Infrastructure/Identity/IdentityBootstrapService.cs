using FortuneInternalData.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FortuneInternalData.Infrastructure.Identity;

public class IdentityBootstrapService
{
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityBootstrapService> _logger;

    public IdentityBootstrapService(
        UserManager<IdentityApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<IdentityBootstrapService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnsureSuperadminAsync()
    {
        // Read from config (which includes environment variables via standard ASP.NET Core config)
        var email = _configuration["Bootstrap:SuperadminEmail"]
                    ?? Environment.GetEnvironmentVariable("BOOTSTRAP_SUPERADMIN_EMAIL");
        var password = _configuration["Bootstrap:SuperadminPassword"]
                       ?? Environment.GetEnvironmentVariable("BOOTSTRAP_SUPERADMIN_PASSWORD");
        var name = _configuration["Bootstrap:SuperadminName"]
                   ?? Environment.GetEnvironmentVariable("BOOTSTRAP_SUPERADMIN_NAME")
                   ?? "System Administrator";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("No bootstrap superadmin credentials provided. Skipping.");
            return;
        }

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            _logger.LogInformation("Bootstrap superadmin '{Email}' already exists. Skipping.", email);
            return;
        }

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
            _logger.LogInformation("Bootstrap superadmin '{Email}' created successfully.", email);
        }
        else
        {
            _logger.LogError("Failed to create bootstrap superadmin '{Email}': {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
