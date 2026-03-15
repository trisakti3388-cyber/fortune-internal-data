namespace FortuneInternalData.Application.Interfaces;

public interface IIdentityService
{
    Task<bool> PasswordSignInAsync(string email, string password, bool rememberMe);
    Task<bool> VerifyTwoFactorCodeAsync(string code);
    Task SignOutAsync();
}
