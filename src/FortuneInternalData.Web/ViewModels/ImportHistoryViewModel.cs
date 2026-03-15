using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Web.ViewModels;

public class ImportHistoryViewModel
{
    public IReadOnlyList<ImportBatchListItemDto> Items { get; set; } = Array.Empty<ImportBatchListItemDto>();
}
