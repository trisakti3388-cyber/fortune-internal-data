namespace FortuneInternalData.Web.ViewModels;

public class BatchUpdateViewModel
{
    public List<ulong> SelectedIds { get; set; } = new();
    public string? Status { get; set; }
    public string? WhatsappStatus { get; set; }
    public string? AgentName { get; set; }
    public string? Remark { get; set; }
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
    // For Superadmin only. Empty string = keep existing, "__unassign__" = unassign, otherwise = assign to user ID
    public string? AssignedUserId { get; set; }
}
