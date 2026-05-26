using System.Threading.Channels;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Services.Connection;

public class MessagesQueue
{
    private readonly Channel<SocketMessage> _channel = Channel.CreateBounded<SocketMessage>(20);

    public async Task<SocketMessage> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }

    public async Task EnqueueAsync(
        SocketMessage message,
        CancellationToken cancellationToken = default
    )
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public bool Enqueue(SocketMessage message)
    {
        return _channel.Writer.TryWrite(message);
    }
}
