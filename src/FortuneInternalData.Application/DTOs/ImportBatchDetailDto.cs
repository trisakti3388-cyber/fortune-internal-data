namespace FortuneInternalData.Application.DTOs;

public class ImportBatchDetailDto
{
    public ulong BatchId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ImportPreviewSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<ImportRowPreviewDto> Rows { get; set; } = Array.Empty<ImportRowPreviewDto>();
}
