namespace FortuneInternalData.Application.Interfaces;

public interface IImportService
{
    Task ValidateUploadAsync(string storedFilePath, CancellationToken cancellationToken = default);
    Task ConfirmImportAsync(ulong batchId, ulong userId, CancellationToken cancellationToken = default);
}
