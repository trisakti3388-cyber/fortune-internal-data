using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IImportQueryService
{
    Task<IReadOnlyList<ImportBatchListItemDto>> GetRecentBatchesAsync(CancellationToken cancellationToken = default);
    Task<ImportBatchDetailDto?> GetBatchDetailAsync(ulong batchId, int page = 1, int pageSize = 100, string? statusFilter = null, CancellationToken cancellationToken = default);
    Task<ImportBatchStatusDto?> GetBatchStatusAsync(ulong batchId, CancellationToken cancellationToken = default);
}
