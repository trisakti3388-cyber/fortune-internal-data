using System.ComponentModel.DataAnnotations;

namespace FortuneInternalData.Web.ViewModels;

public class ChangeRoleViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "New Role")]
    public string NewRole { get; set; } = string.Empty;
}
