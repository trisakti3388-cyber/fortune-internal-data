using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Infrastructure.Identity;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Services;

public class ImportQueryService : IImportQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityApplicationUser> _userManager;

    public ImportQueryService(ApplicationDbContext dbContext, UserManager<IdentityApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<ImportBatchListItemDto>> GetRecentBatchesAsync(CancellationToken cancellationToken = default)
    {
        var batches = await _dbContext.ImportBatches
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var result = new List<ImportBatchListItemDto>();
        foreach (var b in batches)
        {
            var user = await _userManager.FindByIdAsync(b.UploadedByUserId);
            result.Add(new ImportBatchListItemDto
            {
                Id = b.Id,
                FileName = b.FileName,
                UploadedByName = user?.FullName ?? "Unknown",
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                TotalRows = b.TotalRows,
                NewRows = b.NewRows,
                ExistingRows = b.ExistingRows,
                InvalidRows = b.InvalidRows,
                DuplicateRows = b.DuplicateRows
            });
        }

        return result;
    }

    public async Task<ImportBatchDetailDto?> GetBatchDetailAsync(ulong batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.ImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == batchId, cancellationToken);

        if (batch == null) return null;

        var user = await _userManager.FindByIdAsync(batch.UploadedByUserId);

        var rows = await _dbContext.ImportBatchRows
            .AsNoTracking()
            .Where(x => x.BatchId == batchId)
            .OrderBy(x => x.Id)
            .Select(x => new ImportRowPreviewDto
            {
                Seq = x.Seq,
                RawPhoneNumber = x.RawPhoneNumber,
                NormalizedPhoneNumber = x.NormalizedPhoneNumber,
                Remark = x.Remark,
                RowStatus = x.RowStatus,
                Message = x.Message
            })
            .ToListAsync(cancellationToken);

        return new ImportBatchDetailDto
        {
            BatchId = batch.Id,
            FileName = batch.FileName,
            UploadedByName = user?.FullName ?? "Unknown",
            Status = batch.Status,
            CreatedAt = batch.CreatedAt,
            Summary = new ImportPreviewSummaryDto
            {
                TotalRows = batch.TotalRows,
                NewRows = batch.NewRows,
                ExistingRows = batch.ExistingRows,
                InvalidRows = batch.InvalidRows,
                DuplicateRows = batch.DuplicateRows
            },
            Rows = rows
        };
    }
}
