namespace FortuneInternalData.Domain.Entities;

public class ImportBatch : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string? StoredFilePath { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int NewRows { get; set; }
    public int ExistingRows { get; set; }
    public int InvalidRows { get; set; }
    public int DuplicateRows { get; set; }
    public string Status { get; set; } = "pending";
}
