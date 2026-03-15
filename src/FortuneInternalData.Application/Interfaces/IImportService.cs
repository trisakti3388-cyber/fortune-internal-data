namespace FortuneInternalData.Application.Interfaces;

public interface IImportService
{
    Task<ulong> ValidateAndCreateBatchAsync(string storedFilePath, string originalFileName, string uploadedByUserId, CancellationToken cancellationToken = default);
    Task ConfirmImportAsync(ulong batchId, string userId, CancellationToken cancellationToken = default);
    Task CancelImportAsync(ulong batchId, string userId, CancellationToken cancellationToken = default);
}
