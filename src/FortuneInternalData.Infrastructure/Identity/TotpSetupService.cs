using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Infrastructure.Identity;

public class TotpSetupService : ITotpSetupService
{
    public Task<TotpSetupResult> GenerateSetupAsync(string userId)
    {
        // TODO: Implement with Otp.NET and QRCoder.
        return Task.FromResult(new TotpSetupResult
        {
            SecretKey = "TODO-SECRET",
            ManualEntryKey = "TODO-MANUAL-ENTRY",
            QrCodeUri = "otpauth://totp/FortuneInternalData:user@example.com?secret=TODO-SECRET&issuer=FortuneInternalData"
        });
    }

    public Task<bool> VerifyAndEnableAsync(string userId, string code)
    {
        // TODO: Validate code and enable authenticator for the user.
        return Task.FromResult(true);
    }
}
