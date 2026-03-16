namespace FortuneInternalData.Web.ViewModels;

public class BatchUpdateViewModel
{
    public List<ulong> SelectedIds { get; set; } = new();
    public string? Status { get; set; }
    public string? WhatsappStatus { get; set; }
}
