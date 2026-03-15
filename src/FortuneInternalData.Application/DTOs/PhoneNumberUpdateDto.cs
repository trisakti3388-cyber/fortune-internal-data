namespace FortuneInternalData.Application.DTOs;

public class PhoneNumberUpdateDto
{
    public ulong Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? WhatsappStatus { get; set; }
    public string? Remark { get; set; }
}
