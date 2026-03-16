namespace FortuneInternalData.Application.DTOs;

public class ImportBatchStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public string? ErrorMessage { get; set; }
}
