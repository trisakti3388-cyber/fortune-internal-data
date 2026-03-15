using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IImportQueryService
{
    Task<IReadOnlyList<ImportBatchListItemDto>> GetRecentBatchesAsync(CancellationToken cancellationToken = default);
    Task<ImportBatchDetailDto?> GetBatchDetailAsync(ulong batchId, CancellationToken cancellationToken = default);
}
