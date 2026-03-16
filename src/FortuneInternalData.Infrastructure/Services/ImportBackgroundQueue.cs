using System.Threading.Channels;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Infrastructure.Services;

public class ImportBackgroundQueue : IImportBackgroundQueue
{
    private readonly Channel<ulong> _channel = Channel.CreateUnbounded<ulong>(
        new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(ulong batchId) => _channel.Writer.TryWrite(batchId);

    public ValueTask<ulong> DequeueAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAsync(cancellationToken);
}
