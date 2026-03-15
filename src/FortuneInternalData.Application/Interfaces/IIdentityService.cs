namespace FortuneInternalData.Application.Interfaces;

public enum SignInResultType
{
    Success,
    RequiresTwoFactor,
    LockedOut,
    Failed
}

public interface IIdentityService
{
    Task<SignInResultType> PasswordSignInAsync(string email, string password, bool rememberMe);
    Task<bool> VerifyTwoFactorCodeAsync(string code);
    Task SignOutAsync();
    Task<string?> GetCurrentUserIdAsync();
    Task<string?> GetTwoFactorUserIdAsync();
    Task<bool> UserHasTwoFactorEnabledAsync(string userId);
}
