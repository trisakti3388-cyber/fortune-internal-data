namespace FortuneInternalData.Application.DTOs;

public class TotpSetupResultDto
{
    public string SecretKey { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
    public string QrCodeDataUri { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
}
