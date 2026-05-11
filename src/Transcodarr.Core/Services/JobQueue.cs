using System.Threading.Channels;
using Transcodarr.Core.Common.Models;

namespace Transcodarr.Core.Services;

public class JobQueue
{
    private readonly Channel<TranscodeJobRequest> _channel = Channel.CreateBounded<TranscodeJobRequest>(200);

    public ChannelReader<TranscodeJobRequest> Reader => _channel.Reader;
    public ChannelWriter<TranscodeJobRequest> Writer => _channel.Writer;
}