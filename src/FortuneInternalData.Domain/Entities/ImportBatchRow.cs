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
    public string? UpdateStatus { get; set; } // target phone status for update batches
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
    public string? Web11 { get; set; }
    public string? Web12 { get; set; }
    public string? Web13 { get; set; }
    public string? Web14 { get; set; }
    public string? Web15 { get; set; }
    public string? Web16 { get; set; }
    public string? Web17 { get; set; }
    public string? Web18 { get; set; }
    public string? Web19 { get; set; }
    public string? Web20 { get; set; }
    public string? AssignedUserId { get; set; }
}
