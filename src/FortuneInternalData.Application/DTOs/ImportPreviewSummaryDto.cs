namespace FortuneInternalData.Application.DTOs;

public class ImportPreviewSummaryDto
{
    public int TotalRows { get; set; }
    public int NewRows { get; set; }
    public int ExistingRows { get; set; }
    public int InvalidRows { get; set; }
    public int DuplicateRows { get; set; }
}
