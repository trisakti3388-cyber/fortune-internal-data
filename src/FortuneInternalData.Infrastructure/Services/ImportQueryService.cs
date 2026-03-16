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

    public async Task<ImportBatchDetailDto?> GetBatchDetailAsync(
        ulong batchId,
        int page = 1,
        int pageSize = 100,
        string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.ImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == batchId, cancellationToken);

        if (batch == null) return null;

        var user = await _userManager.FindByIdAsync(batch.UploadedByUserId);

        var query = _dbContext.ImportBatchRows
            .AsNoTracking()
            .Where(x => x.BatchId == batchId);

        if (!string.IsNullOrEmpty(statusFilter))
            query = query.Where(x => x.RowStatus == statusFilter);

        var totalRowCount = await query.CountAsync(cancellationToken);

        page = Math.Max(1, page);
        pageSize = pageSize is 50 or 100 or 500 or 1000 ? pageSize : 100;

        var rows = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            ProcessedRows = batch.ProcessedRows,
            ErrorMessage = batch.ErrorMessage,
            Summary = new ImportPreviewSummaryDto
            {
                TotalRows = batch.TotalRows,
                NewRows = batch.NewRows,
                ExistingRows = batch.ExistingRows,
                InvalidRows = batch.InvalidRows,
                DuplicateRows = batch.DuplicateRows
            },
            Rows = rows,
            Page = page,
            PageSize = pageSize,
            TotalRowCount = totalRowCount,
            StatusFilter = statusFilter
        };
    }

    public async Task<ImportBatchStatusDto?> GetBatchStatusAsync(ulong batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.ImportBatches
            .AsNoTracking()
            .Select(b => new { b.Id, b.Status, b.TotalRows, b.ProcessedRows, b.ErrorMessage })
            .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

        if (batch == null) return null;

        return new ImportBatchStatusDto
        {
            Status = batch.Status,
            TotalRows = batch.TotalRows,
            ProcessedRows = batch.ProcessedRows,
            ErrorMessage = batch.ErrorMessage
        };
    }
}
