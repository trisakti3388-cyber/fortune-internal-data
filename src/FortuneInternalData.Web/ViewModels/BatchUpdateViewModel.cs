namespace FortuneInternalData.Web.ViewModels;

public class BatchUpdateViewModel
{
    public List<ulong> SelectedIds { get; set; } = new();
    public string? Status { get; set; }
    public string? WhatsappStatus { get; set; }
    public string? AgentName { get; set; }
    public string? Remark { get; set; }
    public string? Reference { get; set; }
}
