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
    public string? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
}
