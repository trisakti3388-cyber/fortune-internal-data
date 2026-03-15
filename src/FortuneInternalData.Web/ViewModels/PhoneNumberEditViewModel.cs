using System.ComponentModel.DataAnnotations;
using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Web.ViewModels;

public class PhoneNumberEditViewModel
{
    public ulong Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;

    public string? WhatsappStatus { get; set; }

    [StringLength(500)]
    public string? Remark { get; set; }
}
