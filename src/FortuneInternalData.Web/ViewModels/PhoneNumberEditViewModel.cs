using System.ComponentModel.DataAnnotations;

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

    [StringLength(100)]
    public string? AgentName { get; set; }

    [StringLength(255)]
    public string? Reference { get; set; }

    public string? Web1 { get; set; }
    public string? Web2 { get; set; }
    public string? Web3 { get; set; }
    public string? Web4 { get; set; }
    public string? Web5 { get; set; }
    public string? Web6 { get; set; }
    public string? Web7 { get; set; }
    public string? Web8 { get; set; }
    public string? Web9 { get; set; }
    public string? Web10 { get; set; }
}
