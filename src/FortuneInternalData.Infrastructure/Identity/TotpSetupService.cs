using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OtpNet;
using QRCoder;

namespace FortuneInternalData.Infrastructure.Identity;

public class TotpSetupService : ITotpSetupService
{
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public TotpSetupService(UserManager<IdentityApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<TotpSetupResultDto> GenerateSetupAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        // Generate a new authenticator key
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user)
            ?? throw new InvalidOperationException("Failed to generate authenticator key.");

        var issuer = _configuration["AppSettings:TotpIssuer"] ?? "Fortune Internal Data";
        var email = user.Email ?? user.UserName ?? "user";

        var otpAuthUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={unformattedKey}&issuer={Uri.EscapeDataString(issuer)}&digits=6";

        // Generate QR code as base64 PNG data URI
        string qrCodeDataUri;
        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(otpAuthUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(5);
            qrCodeDataUri = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }

        return new TotpSetupResultDto
        {
            SecretKey = unformattedKey,
            ManualEntryKey = FormatKey(unformattedKey),
            QrCodeDataUri = qrCodeDataUri,
            QrCodeUri = otpAuthUri
        };
    }

    public async Task<bool> VerifyAndEnableAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (!isValid)
            return false;

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        return true;
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new System.Text.StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }
        return result.ToString().TrimEnd();
    }
}
