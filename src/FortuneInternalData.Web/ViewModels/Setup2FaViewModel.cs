using System.ComponentModel.DataAnnotations;

namespace FortuneInternalData.Web.ViewModels;

public class Setup2FaViewModel
{
    public string ManualEntryKey { get; set; } = string.Empty;
    public string QrCodeDataUri { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public bool IsAlreadyEnabled { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    [Display(Name = "Verification Code")]
    public string Code { get; set; } = string.Empty;
}
