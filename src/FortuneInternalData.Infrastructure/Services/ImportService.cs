using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Domain.Entities;

namespace FortuneInternalData.Infrastructure.Services;

public class ImportService : IImportService
{
    private readonly IImportFileParser _importFileParser;
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;
    private readonly IImportBatchRepository _importBatchRepository;
    private readonly IImportBatchRowRepository _importBatchRowRepository;
    private readonly IPhoneNumberRepository _phoneNumberRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportService(
        IImportFileParser importFileParser,
        IPhoneNumberValidationService phoneNumberValidationService,
        IImportBatchRepository importBatchRepository,
        IImportBatchRowRepository importBatchRowRepository,
        IPhoneNumberRepository phoneNumberRepository,
        IActivityLogRepository activityLogRepository,
        IUnitOfWork unitOfWork)
    {
        _importFileParser = importFileParser;
        _phoneNumberValidationService = phoneNumberValidationService;
        _importBatchRepository = importBatchRepository;
        _importBatchRowRepository = importBatchRowRepository;
        _phoneNumberRepository = phoneNumberRepository;
        _activityLogRepository = activityLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ValidateUploadAsync(string storedFilePath, CancellationToken cancellationToken = default)
    {
        var parsedRows = await _importFileParser.ParseAsync(storedFilePath, cancellationToken);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var batchRows = new List<ImportBatchRow>();

        foreach (var row in parsedRows)
        {
            var normalized = _phoneNumberValidationService.Normalize(row.PhoneNumber);
            var status = ImportRowStatuses.New;
            string? message = null;

            if (!_phoneNumberValidationService.IsValid(normalized))
            {
                status = ImportRowStatuses.Invalid;
                message = "Phone number must be 10-14 digits only.";
            }
            else if (!seen.Add(normalized))
            {
                status = ImportRowStatuses.DuplicateFile;
                message = "Duplicate phone number in uploaded file.";
            }
            else if (await _phoneNumberRepository.ExistsAsync(normalized, cancellationToken))
            {
                status = ImportRowStatuses.Existing;
                message = "Phone number already exists.";
            }

            batchRows.Add(new ImportBatchRow
            {
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

        // TODO: Link rows to a persisted import batch created before validation.
        await _importBatchRowRepository.AddRangeAsync(batchRows, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmImportAsync(ulong batchId, ulong userId, CancellationToken cancellationToken = default)
    {
        var batch = await _importBatchRepository.GetByIdAsync(batchId, cancellationToken)
            ?? throw new InvalidOperationException("Import batch not found.");

        var newRows = await _importBatchRowRepository.GetNewRowsAsync(batchId, cancellationToken);
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

        await _phoneNumberRepository.AddRangeAsync(entities, cancellationToken);

        batch.Status = "confirmed";
        batch.UpdatedAt = DateTime.UtcNow;
        await _importBatchRepository.UpdateAsync(batch, cancellationToken);

        await _activityLogRepository.AddAsync(new ActivityLog
        {
            UserId = userId,
            Action = "confirm_import",
            TargetType = "import_batches",
            TargetId = batch.Id,
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { ImportedCount = entities.Count }),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
