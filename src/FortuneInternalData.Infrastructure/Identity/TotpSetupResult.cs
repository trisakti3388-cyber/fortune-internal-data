namespace FortuneInternalData.Infrastructure.Identity;

public class TotpSetupResult
{
    public string SecretKey { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
}
