using FortuneInternalData.Domain.Entities;

namespace FortuneInternalData.Application.Interfaces;

public interface IImportBatchRepository
{
    Task<ImportBatch?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task AddAsync(ImportBatch batch, CancellationToken cancellationToken = default);
    Task UpdateAsync(ImportBatch batch, CancellationToken cancellationToken = default);
}
