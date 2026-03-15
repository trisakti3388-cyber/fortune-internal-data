using FortuneInternalData.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FortuneInternalData.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly SignInManager<IdentityApplicationUser> _signInManager;

    public IdentityService(SignInManager<IdentityApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<bool> PasswordSignInAsync(string email, string password, bool rememberMe)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);
        return result.Succeeded || result.RequiresTwoFactor;
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(string code)
    {
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent: false, rememberClient: false);
        return result.Succeeded;
    }

    public Task SignOutAsync()
        => _signInManager.SignOutAsync();
}
