using System.ComponentModel.DataAnnotations;

namespace FortuneInternalData.Web.ViewModels;

/// <summary>
/// Used for forced password change (no current password required - admin reset).
/// </summary>
public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
