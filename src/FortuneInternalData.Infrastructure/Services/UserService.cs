using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<IdentityApplicationUser> _userManager;

    public UserService(UserManager<IdentityApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Success, string[] Errors)> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default)
    {
        var user = new IdentityApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.Name,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description).ToArray());

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
            return (false, roleResult.Errors.Select(e => e.Description).ToArray());

        return (true, Array.Empty<string>());
    }

    public async Task<(bool Success, string[] Errors)> ToggleActiveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found." });

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description).ToArray());

        return (true, Array.Empty<string>());
    }

    public async Task<(bool Success, string[] Errors)> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description).ToArray());

        return (true, Array.Empty<string>());
    }
}
