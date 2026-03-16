using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IUserService
{
    Task<(bool Success, string[] Errors)> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default);
    Task<(bool Success, string[] Errors)> ToggleActiveAsync(string userId, CancellationToken cancellationToken = default);
    Task<(bool Success, string[] Errors)> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);
    Task<(bool Success, string[] Errors)> ChangeRoleAsync(string userId, string newRole, CancellationToken cancellationToken = default);
    Task<(bool Success, string[] Errors)> UnlockUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<(bool Success, string[] Errors)> Reset2FaAsync(string userId, CancellationToken cancellationToken = default);
    Task<(bool Success, string[] Errors)> SetMustChangePasswordAsync(string userId, bool value, CancellationToken cancellationToken = default);
}
