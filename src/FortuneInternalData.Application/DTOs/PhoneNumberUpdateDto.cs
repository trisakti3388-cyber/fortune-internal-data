namespace FortuneInternalData.Application.DTOs;

public class PhoneNumberUpdateDto
{
    public ulong Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? WhatsappStatus { get; set; }
    public string? Remark { get; set; }
    public string? AgentName { get; set; }
    public string? Reference { get; set; }
}
