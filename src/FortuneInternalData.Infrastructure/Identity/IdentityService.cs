using FortuneInternalData.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FortuneInternalData.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly SignInManager<IdentityApplicationUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityService(
        UserManager<IdentityApplicationUser> userManager,
        SignInManager<IdentityApplicationUser> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SignInResultType> PasswordSignInAsync(string email, string password, bool rememberMe)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
            return SignInResultType.Failed;

        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
            return SignInResultType.Success;
        if (result.RequiresTwoFactor)
            return SignInResultType.RequiresTwoFactor;
        if (result.IsLockedOut)
            return SignInResultType.LockedOut;

        return SignInResultType.Failed;
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(string code)
    {
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent: false, rememberClient: false);
        return result.Succeeded;
    }

    public Task SignOutAsync()
        => _signInManager.SignOutAsync();

    public Task<string?> GetCurrentUserIdAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Task.FromResult(userId);
    }

    public async Task<string?> GetTwoFactorUserIdAsync()
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        return user?.Id;
    }

    public async Task<bool> UserHasTwoFactorEnabledAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.GetTwoFactorEnabledAsync(user);
    }
}
