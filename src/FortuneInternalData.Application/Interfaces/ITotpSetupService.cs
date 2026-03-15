using FortuneInternalData.Infrastructure.Identity;

namespace FortuneInternalData.Application.Interfaces;

public interface ITotpSetupService
{
    Task<TotpSetupResult> GenerateSetupAsync(string userId);
    Task<bool> VerifyAndEnableAsync(string userId, string code);
}
