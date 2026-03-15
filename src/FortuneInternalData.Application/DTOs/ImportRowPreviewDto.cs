namespace FortuneInternalData.Application.DTOs;

public class ImportRowPreviewDto
{
    public string? Seq { get; set; }
    public string? RawPhoneNumber { get; set; }
    public string? NormalizedPhoneNumber { get; set; }
    public string? Remark { get; set; }
    public string RowStatus { get; set; } = string.Empty;
    public string? Message { get; set; }
}
