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

    public Task<ulong> CreatePendingBatchAsync(
        string storedFilePath,
        string originalFileName,
        string uploadedByUserId,
        CancellationToken cancellationToken = default)
        => CreatePendingBatchAsync(storedFilePath, originalFileName, uploadedByUserId, "import", null, cancellationToken);

    public Task<ulong> CreatePendingBatchAsync(
        string storedFilePath,
        string originalFileName,
        string uploadedByUserId,
        string batchType,
        CancellationToken cancellationToken = default)
        => CreatePendingBatchAsync(storedFilePath, originalFileName, uploadedByUserId, batchType, null, cancellationToken);

    public async Task<ulong> CreatePendingBatchAsync(
        string storedFilePath,
        string originalFileName,
        string uploadedByUserId,
        string batchType,
        string? assignedUserId,
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
            BatchType = batchType,
            AssignedUserId = assignedUserId,
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

        switch (batch.BatchType)
        {
            case "update":
            case "web_status":
                await ProcessUpdateBatchAsync(batch, cancellationToken);
                break;
            case "delete":
                await ProcessDeleteBatchAsync(batch, cancellationToken);
                break;
            default:
                await ProcessImportBatchAsync(batch, cancellationToken);
                break;
        }
    }

    private async Task ProcessImportBatchAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        try
        {
            var parser = _parserFactory.GetParser(batch.StoredFilePath ?? string.Empty);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int newCount = 0, existingCount = 0, invalidCount = 0, duplicateCount = 0, totalRows = 0;

            await foreach (var chunk in parser.ParseInChunksAsync(batch.StoredFilePath!, 10000, cancellationToken))
            {
                var normalized = chunk.Select(r => (
                    row: r,
                    norm: _phoneNumberValidationService.Normalize(r.PhoneNumber)
                )).ToList();

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
                        message = "Phone number must be 8-20 digits only.";
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
                        BatchId = batch.Id,
                        Seq = row.Seq,
                        RawPhoneNumber = row.PhoneNumber,
                        NormalizedPhoneNumber = norm,
                        Remark = row.Remark,
                        WhatsappStatus = row.WhatsappStatus,
                        AgentName = row.AgentName,
                        Reference = row.Reference,
                        UpdateStatus = row.UpdateStatus,
                        Web1 = row.Web1, Web2 = row.Web2, Web3 = row.Web3, Web4 = row.Web4, Web5 = row.Web5,
                        Web6 = row.Web6, Web7 = row.Web7, Web8 = row.Web8, Web9 = row.Web9, Web10 = row.Web10,
                        AssignedUserId = batch.AssignedUserId,
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
            await SetBatchError(batch.Id, ex);
            throw;
        }
    }

    private async Task ProcessUpdateBatchAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        try
        {
            var parser = _parserFactory.GetParser(batch.StoredFilePath ?? string.Empty);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int matchedCount = 0, notFoundCount = 0, invalidCount = 0, duplicateCount = 0, totalRows = 0;

            await foreach (var chunk in parser.ParseInChunksAsync(batch.StoredFilePath!, 10000, cancellationToken))
            {
                var normalized = chunk.Select(r => (
                    row: r,
                    norm: _phoneNumberValidationService.Normalize(r.PhoneNumber)
                )).ToList();

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
                        message = "Invalid phone number format.";
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
                        status = ImportRowStatuses.Matched;
                        message = null;
                        matchedCount++;
                    }
                    else
                    {
                        status = ImportRowStatuses.NotFound;
                        message = "Phone number not found in system.";
                        notFoundCount++;
                    }

                    batchRows.Add(new ImportBatchRow
                    {
                        BatchId = batch.Id,
                        Seq = row.Seq,
                        RawPhoneNumber = row.PhoneNumber,
                        NormalizedPhoneNumber = norm,
                        Remark = row.Remark,
                        WhatsappStatus = row.WhatsappStatus,
                        AgentName = row.AgentName,
                        Reference = row.Reference,
                        UpdateStatus = row.UpdateStatus,
                        Web1 = row.Web1, Web2 = row.Web2, Web3 = row.Web3, Web4 = row.Web4, Web5 = row.Web5,
                        Web6 = row.Web6, Web7 = row.Web7, Web8 = row.Web8, Web9 = row.Web9, Web10 = row.Web10,
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
            batch.ExistingRows = matchedCount;   // matched = will be updated
            batch.NewRows = notFoundCount;        // not found = no action
            batch.InvalidRows = invalidCount;
            batch.DuplicateRows = duplicateCount;
            batch.ProcessedRows = totalRows;
            batch.Status = "pending";
            batch.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await SetBatchError(batch.Id, ex);
            throw;
        }
    }

    private async Task ProcessDeleteBatchAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        try
        {
            var parser = _parserFactory.GetParser(batch.StoredFilePath ?? string.Empty);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int foundCount = 0, notFoundCount = 0, invalidCount = 0, duplicateCount = 0, totalRows = 0;

            await foreach (var chunk in parser.ParseInChunksAsync(batch.StoredFilePath!, 10000, cancellationToken))
            {
                var normalized = chunk.Select(r => (
                    row: r,
                    norm: _phoneNumberValidationService.Normalize(r.PhoneNumber)
                )).ToList();

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
                        message = "Invalid phone number format.";
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
                        status = ImportRowStatuses.Found;
                        message = null;
                        foundCount++;
                    }
                    else
                    {
                        status = ImportRowStatuses.NotFound;
                        message = "Phone number not found in system.";
                        notFoundCount++;
                    }

                    batchRows.Add(new ImportBatchRow
                    {
                        BatchId = batch.Id,
                        Seq = row.Seq,
                        RawPhoneNumber = row.PhoneNumber,
                        NormalizedPhoneNumber = norm,
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
            batch.ExistingRows = foundCount;     // found = will be deleted
            batch.NewRows = notFoundCount;       // not found = no action
            batch.InvalidRows = invalidCount;
            batch.DuplicateRows = duplicateCount;
            batch.ProcessedRows = totalRows;
            batch.Status = "pending";
            batch.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await SetBatchError(batch.Id, ex);
            throw;
        }
    }

    public async Task ConfirmImportAsync(ulong batchId, string userId, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.ImportBatches.FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken)
            ?? throw new InvalidOperationException("Import batch not found.");

        if (batch.Status != "pending")
            throw new InvalidOperationException($"Import batch is in '{batch.Status}' status and cannot be confirmed.");

        switch (batch.BatchType)
        {
            case "update":
                await ConfirmUpdateImportAsync(batch, userId, cancellationToken);
                break;
            case "web_status":
                await ConfirmWebStatusImportAsync(batch, userId, cancellationToken);
                break;
            case "delete":
                await ConfirmDeleteImportAsync(batch, userId, cancellationToken);
                break;
            default:
                await ConfirmRegularImportAsync(batch, userId, cancellationToken);
                break;
        }
    }

    private async Task ConfirmRegularImportAsync(ImportBatch batch, string userId, CancellationToken cancellationToken)
    {
        var sql = @"
INSERT INTO phone_numbers (seq, phone_number, remark, status, whatsapp_status, agent_name, reference, web1, web2, web3, web4, web5, web6, web7, web8, web9, web10, assigned_user_id, upload_date, modified_date, created_at, updated_at)
SELECT seq, normalized_phone_number, remark, 'active', whatsapp_status, agent_name, reference, web1, web2, web3, web4, web5, web6, web7, web8, web9, web10, assigned_user_id, NOW(), NOW(), NOW(), NOW()
FROM import_batch_rows
WHERE batch_id = {0} AND row_status = 'new'";

        await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { batch.Id }, cancellationToken);

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

    private async Task ConfirmUpdateImportAsync(ImportBatch batch, string userId, CancellationToken cancellationToken)
    {
        var sql = @"
UPDATE phone_numbers pn
INNER JOIN import_batch_rows ibr ON ibr.normalized_phone_number = pn.phone_number
SET
  pn.whatsapp_status = CASE WHEN ibr.whatsapp_status IS NOT NULL AND ibr.whatsapp_status != '' THEN ibr.whatsapp_status ELSE pn.whatsapp_status END,
  pn.agent_name = CASE WHEN ibr.agent_name IS NOT NULL AND ibr.agent_name != '' THEN ibr.agent_name ELSE pn.agent_name END,
  pn.remark = CASE WHEN ibr.remark IS NOT NULL AND ibr.remark != '' THEN ibr.remark ELSE pn.remark END,
  pn.reference = CASE WHEN ibr.reference IS NOT NULL AND ibr.reference != '' THEN ibr.reference ELSE pn.reference END,
  pn.status = CASE WHEN ibr.update_status IS NOT NULL AND ibr.update_status != '' THEN ibr.update_status ELSE pn.status END,
  pn.modified_date = NOW(),
  pn.updated_at = NOW()
WHERE ibr.batch_id = {0} AND ibr.row_status = 'matched'";

        var updatedCount = await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { batch.Id }, cancellationToken);

        batch.Status = "confirmed";
        batch.UpdatedAt = DateTime.UtcNow;

        _dbContext.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            Action = "confirm_update_import",
            TargetType = "import_batches",
            TargetId = batch.Id,
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { UpdatedCount = batch.ExistingRows }),
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ConfirmWebStatusImportAsync(ImportBatch batch, string userId, CancellationToken cancellationToken)
    {
        var sql = @"
UPDATE phone_numbers pn
INNER JOIN import_batch_rows ibr ON ibr.normalized_phone_number = pn.phone_number
SET
  pn.web1  = CASE WHEN ibr.web1  IS NOT NULL THEN ibr.web1  ELSE pn.web1  END,
  pn.web2  = CASE WHEN ibr.web2  IS NOT NULL THEN ibr.web2  ELSE pn.web2  END,
  pn.web3  = CASE WHEN ibr.web3  IS NOT NULL THEN ibr.web3  ELSE pn.web3  END,
  pn.web4  = CASE WHEN ibr.web4  IS NOT NULL THEN ibr.web4  ELSE pn.web4  END,
  pn.web5  = CASE WHEN ibr.web5  IS NOT NULL THEN ibr.web5  ELSE pn.web5  END,
  pn.web6  = CASE WHEN ibr.web6  IS NOT NULL THEN ibr.web6  ELSE pn.web6  END,
  pn.web7  = CASE WHEN ibr.web7  IS NOT NULL THEN ibr.web7  ELSE pn.web7  END,
  pn.web8  = CASE WHEN ibr.web8  IS NOT NULL THEN ibr.web8  ELSE pn.web8  END,
  pn.web9  = CASE WHEN ibr.web9  IS NOT NULL THEN ibr.web9  ELSE pn.web9  END,
  pn.web10 = CASE WHEN ibr.web10 IS NOT NULL THEN ibr.web10 ELSE pn.web10 END,
  pn.modified_date = NOW(),
  pn.updated_at = NOW()
WHERE ibr.batch_id = {0} AND ibr.row_status = 'matched'";

        await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { batch.Id }, cancellationToken);

        batch.Status = "confirmed";
        batch.UpdatedAt = DateTime.UtcNow;

        _dbContext.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            Action = "confirm_web_status_import",
            TargetType = "import_batches",
            TargetId = batch.Id,
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { UpdatedCount = batch.ExistingRows }),
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ConfirmDeleteImportAsync(ImportBatch batch, string userId, CancellationToken cancellationToken)
    {
        var sql = @"
DELETE pn FROM phone_numbers pn
INNER JOIN import_batch_rows ibr ON ibr.normalized_phone_number = pn.phone_number
WHERE ibr.batch_id = {0} AND ibr.row_status = 'found'";

        var deletedCount = await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { batch.Id }, cancellationToken);

        batch.Status = "confirmed";
        batch.UpdatedAt = DateTime.UtcNow;

        _dbContext.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            Action = "confirm_delete_import",
            TargetType = "import_batches",
            TargetId = batch.Id,
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { DeletedCount = batch.ExistingRows }),
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

    private async Task SetBatchError(ulong batchId, Exception ex)
    {
        try
        {
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
            // Best-effort
        }
    }

    private async Task BulkInsertBatchRowsAsync(List<ImportBatchRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0) return;

        const int batchSize = 1000;

        for (int offset = 0; offset < rows.Count; offset += batchSize)
        {
            var batch = rows.Skip(offset).Take(batchSize).ToList();

            var sql = new StringBuilder(
                "INSERT INTO import_batch_rows (batch_id, seq, raw_phone_number, normalized_phone_number, remark, whatsapp_status, agent_name, reference, update_status, web1, web2, web3, web4, web5, web6, web7, web8, web9, web10, row_status, message, created_at, updated_at, assigned_user_id) VALUES ");

            var parameters = new List<MySqlConnector.MySqlParameter>();

            for (int j = 0; j < batch.Count; j++)
            {
                var row = batch[j];
                var p = $"@p{j}_";
                if (j > 0) sql.Append(',');
                sql.Append($"({p}0,{p}1,{p}2,{p}3,{p}4,{p}5,{p}6,{p}7,{p}8,{p}9,{p}10,{p}11,{p}12,{p}13,{p}14,{p}15,{p}16,{p}17,{p}18,{p}19,{p}20,{p}21,{p}22,{p}23)");
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}0", row.BatchId));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}1", (object?)row.Seq ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}2", (object?)row.RawPhoneNumber ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}3", (object?)row.NormalizedPhoneNumber ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}4", (object?)row.Remark ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}5", (object?)row.WhatsappStatus ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}6", (object?)row.AgentName ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}7", (object?)row.Reference ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}8", (object?)row.UpdateStatus ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}9", (object?)row.Web1 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}10", (object?)row.Web2 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}11", (object?)row.Web3 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}12", (object?)row.Web4 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}13", (object?)row.Web5 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}14", (object?)row.Web6 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}15", (object?)row.Web7 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}16", (object?)row.Web8 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}17", (object?)row.Web9 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}18", (object?)row.Web10 ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}19", row.RowStatus));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}20", (object?)row.Message ?? DBNull.Value));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}21", row.CreatedAt));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}22", row.UpdatedAt));
                parameters.Add(new MySqlConnector.MySqlParameter($"{p}23", (object?)row.AssignedUserId ?? DBNull.Value));
            }

            await _dbContext.Database.ExecuteSqlRawAsync(sql.ToString(), parameters.ToArray(), cancellationToken);
        }
    }
}
