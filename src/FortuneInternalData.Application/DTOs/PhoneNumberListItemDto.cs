namespace FortuneInternalData.Application.DTOs;

public class PhoneNumberListItemDto
{
    public ulong Id { get; set; }
    public string? Seq { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? WhatsappStatus { get; set; }
    public string? AgentName { get; set; }
    public string? Reference { get; set; }
    public DateTime? UploadDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
