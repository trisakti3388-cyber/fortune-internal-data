namespace FortuneInternalData.Domain.Entities;

public class ImportBatchRow : BaseEntity
{
    public ulong BatchId { get; set; }
    public string? Seq { get; set; }
    public string? RawPhoneNumber { get; set; }
    public string? NormalizedPhoneNumber { get; set; }
    public string? Remark { get; set; }
    public string? WhatsappStatus { get; set; }
    public string? AgentName { get; set; }
    public string? Reference { get; set; }
    public string RowStatus { get; set; } = string.Empty;
    public string? Message { get; set; }
}
