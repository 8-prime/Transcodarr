using System.Threading.Channels;
using Transcodarr.Core.Common.Events;

namespace Transcodarr.Core.Services;

public class LibraryService
{
    private readonly Channel<LibraryChangeEvent> _changeEvents = Channel.CreateBounded<LibraryChangeEvent>(200);

    public ChannelReader<LibraryChangeEvent> Reader => _changeEvents.Reader;

    public async ValueTask UpdateLibraryAsync(LibraryChangeEvent libraryChangeEvent)
    {
        await _changeEvents.Writer.WriteAsync(libraryChangeEvent);
    }
}