using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Repositories;

public class ImportBatchRowRepository : IImportBatchRowRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ImportBatchRowRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRangeAsync(IEnumerable<ImportBatchRow> rows, CancellationToken cancellationToken = default)
        => await _dbContext.ImportBatchRows.AddRangeAsync(rows, cancellationToken);

    public async Task<IReadOnlyList<ImportBatchRow>> GetByBatchIdAsync(ulong batchId, CancellationToken cancellationToken = default)
        => await _dbContext.ImportBatchRows
            .AsNoTracking()
            .Where(x => x.BatchId == batchId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ImportBatchRow>> GetNewRowsAsync(ulong batchId, CancellationToken cancellationToken = default)
        => await _dbContext.ImportBatchRows
            .AsNoTracking()
            .Where(x => x.BatchId == batchId && x.RowStatus == ImportRowStatuses.New)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
}
