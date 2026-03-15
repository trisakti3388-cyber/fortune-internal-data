namespace FortuneInternalData.Application.DTOs;

public class ImportBatchListItemDto
{
    public ulong Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalRows { get; set; }
    public int NewRows { get; set; }
    public int ExistingRows { get; set; }
    public int InvalidRows { get; set; }
    public int DuplicateRows { get; set; }
}
