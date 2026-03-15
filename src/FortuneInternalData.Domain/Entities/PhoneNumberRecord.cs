namespace FortuneInternalData.Domain.Entities;

public class PhoneNumberRecord : BaseEntity
{
    public string? Seq { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public string Status { get; set; } = "active";
    public string? WhatsappStatus { get; set; }
    public DateTime? UploadDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
