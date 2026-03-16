namespace FortuneInternalData.Application.DTOs;

public class ImportBatchDetailDto
{
    public ulong BatchId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ProcessedRows { get; set; }
    public string? ErrorMessage { get; set; }
    public ImportPreviewSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<ImportRowPreviewDto> Rows { get; set; } = Array.Empty<ImportRowPreviewDto>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
    public int TotalRowCount { get; set; }
    public string? StatusFilter { get; set; }
    public string BatchType { get; set; } = "import";
}
