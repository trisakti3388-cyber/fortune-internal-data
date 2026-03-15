using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Application.Services;

public class ImportQueryService : IImportQueryService
{
    public Task<IReadOnlyList<ImportBatchListItemDto>> GetRecentBatchesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with repository-backed query.
        return Task.FromResult<IReadOnlyList<ImportBatchListItemDto>>(new List<ImportBatchListItemDto>());
    }

    public Task<ImportBatchDetailDto?> GetBatchDetailAsync(ulong batchId, CancellationToken cancellationToken = default)
    {
        // TODO: Replace with repository-backed query.
        return Task.FromResult<ImportBatchDetailDto?>(null);
    }
}
