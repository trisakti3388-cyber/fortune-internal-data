using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Services;

public class ImportService : IImportService
{
    private readonly ImportFileParserFactory _parserFactory;
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;
    private readonly ApplicationDbContext _dbContext;

    public ImportService(
        ImportFileParserFactory parserFactory,
        IPhoneNumberValidationService phoneNumberValidationService,
        ApplicationDbContext dbContext)
    {
        _parserFactory = parserFactory;
        _phoneNumberValidationService = phoneNumberValidationService;
        _dbContext = dbContext;
    }

    public async Task<ulong> ValidateAndCreateBatchAsync(string storedFilePath, string originalFileName, string uploadedByUserId, CancellationToken cancellationToken = default)
    {
        var parser = _parserFactory.GetParser(storedFilePath);
        var parsedRows = await parser.ParseAsync(storedFilePath, cancellationToken);

        // Create the batch record first
        var batch = new ImportBatch
        {
            FileName = originalFileName,
            StoredFilePath = storedFilePath,
            UploadedByUserId = uploadedByUserId,
            TotalRows = parsedRows.Count,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ImportBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Validate rows
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var batchRows = new List<ImportBatchRow>();

        // Batch query existing phone numbers for performance
        var allNormalized = parsedRows
            .Select(r => _phoneNumberValidationService.Normalize(r.PhoneNumber))
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .ToList();

        var existingPhones = new HashSet<string>(StringComparer.Ordinal);
        if (allNormalized.Count > 0)
        {
            // Query in batches of 500
            for (int i = 0; i < allNormalized.Count; i += 500)
            {
                var chunk = allNormalized.Skip(i).Take(500).ToList();
                var found = await _dbContext.PhoneNumbers
                    .Where(p => chunk.Contains(p.PhoneNumber))
                    .Select(p => p.PhoneNumber)
                    .ToListAsync(cancellationToken);
                foreach (var f in found)
                    existingPhones.Add(f);
            }
        }

        int newCount = 0, existingCount = 0, invalidCount = 0, duplicateCount = 0;

        foreach (var row in parsedRows)
        {
            var normalized = _phoneNumberValidationService.Normalize(row.PhoneNumber);
            var status = ImportRowStatuses.New;
            string? message = null;

            if (!_phoneNumberValidationService.IsValid(normalized))
            {
                status = ImportRowStatuses.Invalid;
                message = "Phone number must be 10-14 digits only.";
                invalidCount++;
            }
            else if (!seen.Add(normalized))
            {
                status = ImportRowStatuses.DuplicateFile;
                message = "Duplicate phone number in uploaded file.";
                duplicateCount++;
            }
            else if (existingPhones.Contains(normalized))
            {
                status = ImportRowStatuses.Existing;
                message = "Phone number already exists in system.";
                existingCount++;
            }
            else
            {
                newCount++;
            }

            batchRows.Add(new ImportBatchRow
            {
                BatchId = batch.Id,
                Seq = row.Seq,
                RawPhoneNumber = row.PhoneNumber,
                NormalizedPhoneNumber = normalized,
                Remark = row.Remark,
                RowStatus = status,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Update batch counts
        batch.NewRows = newCount;
        batch.ExistingRows = existingCount;
        batch.InvalidRows = invalidCount;
        batch.DuplicateRows = duplicateCount;
        batch.UpdatedAt = DateTime.UtcNow;

        await _dbContext.ImportBatchRows.AddRangeAsync(batchRows, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return batch.Id;
    }

    public async Task ConfirmImportAsync(ulong batchId, string userId, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.ImportBatches.FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken)
            ?? throw new InvalidOperationException("Import batch not found.");

        if (batch.Status != "pending")
            throw new InvalidOperationException($"Import batch is in '{batch.Status}' status and cannot be confirmed.");

        var newRows = await _dbContext.ImportBatchRows
            .Where(x => x.BatchId == batchId && x.RowStatus == ImportRowStatuses.New)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var entities = newRows.Select(x => new PhoneNumberRecord
        {
            Seq = x.Seq,
            PhoneNumber = x.NormalizedPhoneNumber ?? string.Empty,
            Remark = x.Remark,
            Status = PhoneStatuses.Active,
            UploadDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await _dbContext.PhoneNumbers.AddRangeAsync(entities, cancellationToken);

        batch.Status = "confirmed";
        batch.UpdatedAt = DateTime.UtcNow;

        _dbContext.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            Action = "confirm_import",
            TargetType = "import_batches",
            TargetId = batch.Id,
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { ImportedCount = entities.Count }),
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelImportAsync(ulong batchId, string userId, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.ImportBatches.FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken)
            ?? throw new InvalidOperationException("Import batch not found.");

        if (batch.Status != "pending")
            throw new InvalidOperationException($"Import batch is in '{batch.Status}' status and cannot be cancelled.");

        batch.Status = "cancelled";
        batch.UpdatedAt = DateTime.UtcNow;

        _dbContext.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            Action = "cancel_import",
            TargetType = "import_batches",
            TargetId = batch.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
