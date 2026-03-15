using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface ITotpSetupService
{
    Task<TotpSetupResultDto> GenerateSetupAsync(string userId);
    Task<bool> VerifyAndEnableAsync(string userId, string code);
}
