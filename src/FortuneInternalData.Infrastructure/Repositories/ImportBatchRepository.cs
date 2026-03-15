using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Repositories;

public class ImportBatchRepository : IImportBatchRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ImportBatchRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ImportBatch?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default)
        => _dbContext.ImportBatches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(ImportBatch batch, CancellationToken cancellationToken = default)
        => await _dbContext.ImportBatches.AddAsync(batch, cancellationToken);

    public Task UpdateAsync(ImportBatch batch, CancellationToken cancellationToken = default)
    {
        _dbContext.ImportBatches.Update(batch);
        return Task.CompletedTask;
    }
}
