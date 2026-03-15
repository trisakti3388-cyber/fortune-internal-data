using System.ComponentModel.DataAnnotations;

namespace FortuneInternalData.Web.ViewModels;

public class VerifyOtpViewModel
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    [Display(Name = "Authentication Code")]
    public string Code { get; set; } = string.Empty;
}
