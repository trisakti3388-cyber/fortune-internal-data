using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Web.ViewModels;

public class ImportPreviewViewModel
{
    public ulong BatchId { get; set; }
    public ImportPreviewSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<ImportRowPreviewDto> Rows { get; set; } = Array.Empty<ImportRowPreviewDto>();
}
