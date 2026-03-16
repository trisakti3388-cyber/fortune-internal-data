using FortuneInternalData.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FortuneInternalData.Infrastructure.Services;

public class ImportBackgroundWorker : BackgroundService
{
    private readonly IImportBackgroundQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImportBackgroundWorker> _logger;

    public ImportBackgroundWorker(
        IImportBackgroundQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ImportBackgroundWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Import background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            ulong batchId;
            try
            {
                batchId = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            _logger.LogInformation("Processing import batch {BatchId}.", batchId);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var importService = scope.ServiceProvider.GetRequiredService<IImportService>();

            try
            {
                await importService.ProcessBatchAsync(batchId, stoppingToken);
                _logger.LogInformation("Import batch {BatchId} processed successfully.", batchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import batch {BatchId} failed.", batchId);
                // ImportService.ProcessBatchAsync already sets status to "error"
            }
        }

        _logger.LogInformation("Import background worker stopped.");
    }
}
