using System.Text;
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

    public async Task<ulong> CreatePendingBatchAsync(
        string storedFilePath,
        string originalFileName,
        string uploadedByUserId,
        CancellationToken cancellationToken = default)
    {
        var batch = new ImportBatch
        {
            FileName = originalFileName,
            StoredFilePath = storedFilePath,
            UploadedByUserId = uploadedByUserId,
            TotalRows = 0,
            ProcessedRows = 0,
            Status = "processing",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ImportBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return batch.Id;
    }

    public async Task ProcessBatchAsync(ulong batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.ImportBatches.FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken)
            ?? throw new InvalidOperationException($"Import batch {batchId} not found.");

        try
        {
            var parser = _parserFactory.GetParser(batch.StoredFilePath ?? string.Empty);

            // Tracks file-level duplicates across all chunks
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int newCount = 0, existingCount = 0, invalidCount = 0, duplicateCount = 0, totalRows = 0;

            await foreach (var chunk in parser.ParseInChunksAsync(batch.StoredFilePath!, 10000, cancellationToken))
            {
                // Normalize all rows in this chunk
                var normalized = chunk.Select(r => (
                    row: r,
                    norm: _phoneNumberValidationService.Normalize(r.PhoneNumber)
                )).ToList();

                // Determine which normalized phones need a DB existence check:
                // valid, not already seen from previous chunks, distinct within this chunk
                var toCheckInDb = normalized
                    .Where(x => _phoneNumberValidationService.IsValid(x.norm) && !seen.Contains(x.norm!))
                    .Select(x => x.norm!)
                    .Distinct()
                    .ToList();

                var existingInDb = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < toCheckInDb.Count; i += 500)
                {
                    var subChunk = toCheckInDb.Skip(i).Take(500).ToList();
                    var found = await _dbContext.PhoneNumbers
                        .Where(p => subChunk.Contains(p.PhoneNumber))
                        .Select(p => p.PhoneNumber)
                        .ToListAsync(cancellationToken);
                    foreach (var f in found)
                        existingInDb.Add(f);
                }

                var batchRows = new List<ImportBatchRow>(chunk.Count);

                foreach (var (row, norm) in normalized)
                {
                    string status;
                    string? message;

                    if (!_phoneNumberValidationService.IsValid(norm))
                    {
                        status = ImportRowStatuses.Invalid;
                        message = "Phone number must be 10-14 digits only.";
                        invalidCount++;
                    }
                    else if (!seen.Add(norm!))
                    {
                        status = ImportRowStatuses.DuplicateFile;
                        message = "Duplicate phone number in uploaded file.";
                        duplicateCount++;
                    }
                    else if (existingInDb.Contains(norm!))
                    {
                        status = ImportRowStatuses.Existing;
                        message = "Phone number already exists in system.";
                        existingCount++;
                    }
                    else
                    {
                        status = ImportRowStatuses.New;
                        message = null;
                        newCount++;
                    }

                    batchRows.Add(new ImportBatchRow
                    {
                        BatchId = batchId,
                        Seq = row.Seq,
                        RawPhoneNumber = row.PhoneNumber,
                        NormalizedPhoneNumber = norm,
                        Remark = row.Remark,
                        WhatsappStatus = row.WhatsappStatus,
                        AgentName = row.AgentName,
                        Reference = row.Reference,
                        RowStatus = status,
                        Message = message,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                await BulkInsertBatchRowsAsync(batchRows, cancellationToken);

                totalRows += chunk.Count;
                batch.ProcessedRows = totalRows;
                batch.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            batch.TotalRows = totalRows;
            batch.NewRows = newCount;
            batch.ExistingRows = existingCount;
            batch.InvalidRows = invalidCount;
            batch.DuplicateRows = duplicateCount;
            batch.ProcessedRows = totalRows;
            batch.Status = "pending";
            batch.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            try
            {
                // Reload batch in case it was detached by earlier error
                var failedBatch = await _dbContext.ImportBatches
                    .FirstOrDefaultAsync(b => b.Id == batchId, CancellationToken.None);
                if (failedBatch != null)
                {
                    failedBatch.Status = "error";
                    failedBatch.ErrorMessage = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                    failedBatch.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync(CancellationToken.None);
                }
            }
            catch
            {
                // Best-effort error status update — don't mask the original exception
            }
            throw;
        }
    }

    public async Task ConfirmImportAsync(ulong batchId, string userId, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.ImportBatches.FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken)
            ?? throw new InvalidOperationException("Import batch not found.");

        if (batch.Status != "pending")
            throw new InvalidOperationException($"Import batch is in '{batch.Status}' status and cannot be confirmed.");

        // Direct SQL INSERT ... SELECT — no entity loading, handles 500K rows in one statement
        var sql = @"
INSERT INTO phone_numbers (seq, phone_number, remark, status, whatsapp_status, agent_name, reference, upload_date, modified_date, created_at, updated_at)
SELECT seq, normalized_phone_number, remark, 'active', whatsapp_status, agent_name, reference, NOW(), NOW(), NOW(), NOW()
FROM import_batch_rows
WHERE batch_id = {0} AND row_status = 'new'";

        var importedCount = await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { batchId }, cancellationToken);

        batch.Status = "confirmed";
        batch.UpdatedAt = DateTime.UtcNow;

        _dbContext.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            Action = "confirm_import",
            TargetType = "import_batches",
            TargetId = batch.Id,
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { ImportedCount = batch.NewRows }),
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

    private async Task BulkInsertBatchRowsAsync(List<ImportBatchRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0) return;

        const int batchSize = 1000;

        for (int offset = 0; offset < rows.Count; offset += batchSize)
        {
            var batch = rows.Skip(offset).Take(batchSize).ToList();

            var sql = new StringBuilder(
                "INSERT INTO import_batch_rows (batch_id, seq, raw_phone_number, normalized_phone_number, remark, whatsapp_status, agent_name, reference, row_status, message, created_at, updated_at) VALUES ");

            var parameters = new List<MySqlConnector.MySqlParameter>();

            for (int j = 0; j < batch.Count; j++)
            {
                var row = batch[j];
                var prefix = $"@p{j}_";
                if (j > 0) sql.Append(',');
                sql.Append($"({prefix}0,{prefix}1,{prefix}2,{prefix}3,{prefix}4,{prefix}5,{prefix}6,{prefix}7,{prefix}8,{prefix}9,{prefix}10,{prefix}11)");
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}0", row.BatchId));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}1", (object?)row.Seq ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}2", (object?)row.RawPhoneNumber ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}3", (object?)row.NormalizedPhoneNumber ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}4", (object?)row.Remark ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}5", (object?)row.WhatsappStatus ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}6", (object?)row.AgentName ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}7", (object?)row.Reference ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}8", row.RowStatus));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}9", (object?)row.Message ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}10", row.CreatedAt));
                parameters.Add(new MySqlConnector.MySqlParameter($"{prefix}11", row.UpdatedAt));
            }

            await _dbContext.Database.ExecuteSqlRawAsync(sql.ToString(), parameters.ToArray(), cancellationToken);
        }
    }
}
