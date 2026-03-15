using FortuneInternalData.Domain.Entities;

namespace FortuneInternalData.Application.Interfaces;

public interface IImportBatchRowRepository
{
    Task AddRangeAsync(IEnumerable<ImportBatchRow> rows, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImportBatchRow>> GetByBatchIdAsync(ulong batchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImportBatchRow>> GetNewRowsAsync(ulong batchId, CancellationToken cancellationToken = default);
}
