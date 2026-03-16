namespace FortuneInternalData.Application.Interfaces;

public interface IImportBackgroundQueue
{
    void Enqueue(ulong batchId);
    ValueTask<ulong> DequeueAsync(CancellationToken cancellationToken);
}
